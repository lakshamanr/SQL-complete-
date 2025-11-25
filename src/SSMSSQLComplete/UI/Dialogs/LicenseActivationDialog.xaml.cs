using System;
using System.Windows;
using SSMSSQLComplete.Core.Licensing;

namespace SSMSSQLComplete.UI.Dialogs
{
    public partial class LicenseActivationDialog : Window
    {
        public bool WasActivated { get; private set; }
        public Core.Licensing.LicenseInfo ActivatedLicense { get; private set; }

        public LicenseActivationDialog()
        {
            InitializeComponent();
            LoadCurrentState();
        }

        private void LoadCurrentState()
        {
            // Display machine ID
            MachineIdTextBox.Text = LicenseManager.Instance.GetMachineIdForActivation();

            // Check current license status
            var currentLicense = LicenseManager.Instance.GetCurrentLicense();

            if (currentLicense.Type == LicenseType.Trial)
            {
                var trialInfo = TrialManager.Instance.GetTrialInfo();
                ShowStatus($"Current Status: {trialInfo.GetDisplayMessage()}", false);
            }
            else if (currentLicense.IsValid())
            {
                LicenseKeyTextBox.Text = currentLicense.LicenseKey;
                LicensedToTextBox.Text = currentLicense.LicensedTo;
                ShowStatus($"Current license: {currentLicense.GetDisplayStatus()}", false);
                StartTrialButton.IsEnabled = false;
            }
            else
            {
                ShowStatus("No active license. Enter a license key or start a trial.", false);
            }
        }

        private void Activate_Click(object sender, RoutedEventArgs e)
        {
            var licenseKey = LicenseKeyTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(licenseKey))
            {
                ShowStatus("Please enter a license key.", true);
                return;
            }

            var licensedTo = LicensedToTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(licensedTo))
            {
                licensedTo = "Licensed User";
            }

            try
            {
                ActivateButton.IsEnabled = false;
                ActivateButton.Content = "Activating...";

                var result = LicenseManager.Instance.ActivateLicense(licenseKey, licensedTo);

                if (result.IsSuccessful)
                {
                    WasActivated = true;
                    ActivatedLicense = result.LicenseInfo;

                    MessageBox.Show(
                        $"License activated successfully!\n\n" +
                        $"Type: {result.LicenseInfo.Type}\n" +
                        $"Licensed To: {result.LicenseInfo.LicensedTo}\n" +
                        $"Status: {result.LicenseInfo.GetDisplayStatus()}",
                        "Activation Successful",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowStatus($"Activation failed: {result.ErrorMessage}", true);
                    ActivateButton.IsEnabled = true;
                    ActivateButton.Content = "Activate";
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Error: {ex.Message}", true);
                ActivateButton.IsEnabled = true;
                ActivateButton.Content = "Activate";
            }
        }

        private void StartTrial_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "This will start a 30-day trial period. You can only start the trial once.\n\n" +
                "Do you want to start the trial now?",
                "Start Trial",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var trialStarted = TrialManager.Instance.StartTrial();

                if (trialStarted)
                {
                    MessageBox.Show(
                        "Trial period started successfully!\n\n" +
                        "You have 30 days to evaluate SSMS SQL Complete.",
                        "Trial Started",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    WasActivated = true;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowStatus("Trial has already been started or could not be initialized.", true);
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CopyMachineId_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(MachineIdTextBox.Text);
                ShowStatus("Machine ID copied to clipboard", false);
            }
            catch (Exception ex)
            {
                ShowStatus($"Failed to copy: {ex.Message}", true);
            }
        }

        private void LicenseKeyTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Enable/disable activate button based on whether key is entered
            ActivateButton.IsEnabled = !string.IsNullOrWhiteSpace(LicenseKeyTextBox.Text);
        }

        private void ShowStatus(string message, bool isError)
        {
            StatusMessageTextBlock.Text = message;
            StatusMessageTextBlock.Foreground = isError
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red)
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
        }
    }
}
