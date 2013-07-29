﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Windows;
using Microsoft.Win32;

namespace DistrEx.Worker.Service.Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string ServiceName = "Worker";
        private ServiceController service; 

        public MainWindow()
        {
            InitializeComponent();

            InitializeService(); 
        }

        private void InitializeService()
        {
            ServiceController serviceController = ServiceController.GetServices().FirstOrDefault(i => i.ServiceName.Equals(ServiceName));

            if (serviceController == null)
            {
                UpdateStatus("Not installed");
            }
            else
            {
                UpdateStatus("Installed"); 
                service = serviceController;
            }
        }

        private void InstallButtonClick(object sender, RoutedEventArgs e)
        {
            const string commandText = "installutil DistrEx.Worker.Service.exe";
            if (FileName.Text == String.Empty)
            {
                //Output error message 
                return; 
            }
            string command = Path.GetDirectoryName(@FileName.Text) + "\\" + commandText;
            RunCommand(command);

            InitializeService();
        }

        private void StartServiceButtonClick(object sender, RoutedEventArgs e)
        {
            if (service == null)
            {
                UpdateStatus("Not installed");
                return;
            }

            if (service.Status == ServiceControllerStatus.Stopped || service.Status == ServiceControllerStatus.Paused)
            {
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running);
            }
            UpdateStatus(ServiceControllerStatus.Running.ToString());

        }

        private void StopServiceButtonClick(object sender, RoutedEventArgs e)
        {
            if (service == null)
            {
                UpdateStatus("Not installed");
                return; 
            }

            if (service.Status == ServiceControllerStatus.Running)
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped);
            }
            UpdateStatus(ServiceControllerStatus.Stopped.ToString());
        }

        private void UpdateStatus(string status)
        {
            StatusLable.Content = status; 
        }
        
        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            if (service != null)
            {
                UpdateStatus(service.Status.ToString());
            }
            else
            {
                UpdateStatus("Not installed");
            }
        }
        
        private void WindowsServicesClick(object sender, RoutedEventArgs e)
        {
            const string commandText = "/C mmc.exe services.msc";
            RunCommand(commandText);
        }

        private void AutomaticServiceChecked(object sender, RoutedEventArgs e)
        {
            //Set service to automatic
        }

        private static void RunCommand(string command)
        {
            var process = new Process();
            var startInfo = new ProcessStartInfo
            {
                RedirectStandardInput = true,
                UseShellExecute = false,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = command
            };
            process.StartInfo = startInfo;
            process.Start();

            process.WaitForExit();
            process.Close();
        }

        private void UninstallButtonClick(object sender, RoutedEventArgs e)
        {
            const string commandText = "installutil -u DistrEx.Worker.Service";
            if (FileName.Text == String.Empty)
            {
                //Output error message 
                return;
            }
            string command = Path.GetDirectoryName(@FileName.Text) + "\\" + commandText;
            RunCommand(command);
        }

        private void BrowseDirectoryClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select the service file executable",
            };
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                string fileName = openFileDialog.FileName;
                FileName.Text = fileName;
            }
        }
    }
}
