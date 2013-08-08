﻿using System;
using System.Configuration;
using System.Threading;
using DistrEx.Common;
using DistrEx.Common.Exceptions;
using DistrEx.Communication.Service.Executor;
using DistrEx.Coordinator.Interface;
using DistrEx.Coordinator.TargetSpecs;
using DistrEx.Coordinator.Test.Util;
using Microsoft.Test.ApplicationControl;
using NUnit.Framework;

namespace DistrEx.Coordinator.Test.TargetSpecs
{
    [TestFixture]
    public class OnWorkerTest
    {
        private AutomatedApplication _workerProcess;
        private ExecutorCallbackService _callbackHandler;
        private TargetSpec _onWorker;

        private Instruction<int, int> _identity;
        private Instruction<int, int> _haltingIdentity;
        private Instruction<int, int> _uncancellableHaltingIdentity;
        private Instruction<Exception, Exception> _throw;
        
        private TwoPartInstruction<int, int> _twoPartIdentity;

        int _argumentIdentity;
        private Exception _argumentThrow;

        #region setup
        [TestFixtureSetUp]
        public void SetupFixture()
        {
            _workerProcess = ProcessHelper.Start(ConfigurationManager.AppSettings.Get("DistrEx.Coordinator.Test.worker-exe-file"));
            _callbackHandler = new ExecutorCallbackService();
            _onWorker = OnWorker.FromEndpointConfigNames("localhost-assemblyManager", "localhost-executor", _callbackHandler);

            ConfigureOperations();
        }

        private void ConfigureOperations()
        {
            _identity = (ct, p, i) => i;
            _haltingIdentity = (ct, p, i) =>
                {
                    ManualResetEventSlim mres = new ManualResetEventSlim(false);
                    mres.Wait(ct);
                    return i;
                };
            _uncancellableHaltingIdentity = (ct, p, i) =>
            {
                ManualResetEventSlim mres = new ManualResetEventSlim(false);
                mres.Wait();
                return i;
            };
            _throw = (ct, p, e) =>
            {
                throw e;
            };
            _argumentIdentity = 1;
            _argumentThrow = new Exception("Expected");

            _twoPartIdentity = (ct, p, p1, i) =>
                {
                    p1();
                    return i;
                };
        }
        #endregion

        [Test]
        public void SuccessfulOnWorker()
        {
            int expected = _argumentIdentity;
            int actual = Interface.Coordinator.Do(_onWorker.Do(_identity), _argumentIdentity).ResultValue;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Expected")]
        public void FailureOnWorker()
        {
            Exception actual = Interface.Coordinator.Do(_onWorker.Do(_throw), _argumentThrow).ResultValue;
        }

        [Test]
        [ExpectedException(typeof(OperationCanceledException))]
        public void CancelOnWorker()
        {
            var targetedInstruction = _onWorker.Do(_haltingIdentity);
            targetedInstruction.TransportAssemblies();
            var future = targetedInstruction.Invoke(_argumentIdentity);
            future.Cancel();
            future.GetResult();
        }

        [Test]
        [ExpectedException(typeof(AsymmetricTerminationException))]
        public void KillOnWorker()
        {
            var targetedInstruction = _onWorker.Do(_uncancellableHaltingIdentity);
            targetedInstruction.TransportAssemblies();
            var future = targetedInstruction.Invoke(_argumentIdentity);
            future.Cancel();
            future.GetResult();
        }

        [Test]
        public void AsyncTestOnWorker()
        {
            var expected = _argumentIdentity;
            int result = Interface.Coordinator.Do(_onWorker.Do(_twoPartIdentity), _argumentIdentity)
                                              .ThenDo(_onWorker.GetAsyncResult<int>())
                                              .ResultValue;
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        [ExpectedException(typeof(OperationCanceledException))]
        public void TimeoutOnWorker()
        {
            var result = Interface.Coordinator.Do(_onWorker.Do(_haltingIdentity), _argumentIdentity).ResultValue;
        }

        [TestFixtureTearDown]
        public void TeardownFixture()
        {
            _onWorker.ClearEverything();
            ProcessHelper.Stop(_workerProcess);
        }
    }
}
