using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ControllerPriestGUI.Logic;

namespace ControllerPriestGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PriestInput input = new PriestInput();
        SolidColorBrush disconnectedColour = new SolidColorBrush(Color.FromRgb(230, 0, 0));
        SolidColorBrush connectedColour = new SolidColorBrush(Color.FromRgb(0, 230, 0));
        Label[] con_status = new Label[4];
        String strConnected = "CONNECTED";
        String strDisconnected = "DISCONNECTED";
        String strStart = "START";
        String strStop = "STOP";
        String strOutput = "OUTPUT";
        String strMaster = "MASTER";
        String WarnOutputMaster = "You can't set the master controller to be the same as the output controller! Please choose another controller slot!";
        String WarnMasterDisconnect = "No controller is connected on this port! Please choose another controller port to be master.";
        String WarnErrorType = "Controller Error";
        bool emulatedStart = false;
        
        /// <summary>
        /// Initialise the MainWaindow. Setup combobox and setup arrays for referencing later.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            input.Start();
            input.Update();
            PriestInputUpdate();

            con_status[0] = lbl_con_1_status;
            con_status[1] = lbl_con_2_status;
            con_status[2] = lbl_con_3_status;
            con_status[3] = lbl_con_4_status;

            con_master_combo.Items.Add("1");
            con_master_combo.Items.Add("2");
            con_master_combo.Items.Add("3");
            con_master_combo.Items.Add("4");
            con_master_combo.Items.Add("NONE");
            con_master_combo.SelectedIndex = 4;
            lbl_status.Content = "!!!REMEMBER!!! Giving control has changed to R1 + L1 + L3 + R3 !!!REMEMBER!!!";
        }

        /// <summary>
        /// Used when starting or stopping the controller to connect or disconnect the controller that will be used as output.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ActivateEmulatedController(object sender, RoutedEventArgs e)
        {
            if (!emulatedStart)
            {
                var initialCheck = input.CheckConnections();
                input.StartOutputController();
                var secondaryCheck = input.CheckConnections();

                for (int i = 0; i < initialCheck.Length; i++)
                {
                    if (!initialCheck[i] && secondaryCheck[i])
                    {
                        input.Output = i;
                    }
                }

                emulatedStart = true;

                btn_emu_con.Content = strStop;
            }
            else
            {
                emulatedStart = false;
                btn_emu_con.Content = strStart;
                input.StopOutputController();
                input.Output = -1;
            }
        }

        /// <summary>
        /// Update the labels on the window involved with controller connections. Set text and colour.
        /// </summary>
        /// <param name="connections"></param>
        public void UpdateConnectionLabels(bool[] connections)
        {

            for (int i = 0; i < con_status.Length; i++)
            {
                con_status[i].Content = (connections[i] ? strConnected : strDisconnected);
                con_status[i].Foreground = (connections[i] ? connectedColour : disconnectedColour);
            }

            if (input.Output != -1 && connections[input.Output])
                con_status[input.Output].Content = strOutput;
            if (input.Master != -1 && connections[input.Master])
                con_status[input.Master].Content = strMaster;
        }

        /// <summary>
        /// Our background task that helps process the input of controllers. We recheck connection to controllers and also check to see if the
        /// master controller has been changed by PriestInput class. If so we update the ComboBox to reflect the new master controller port.
        /// </summary>
        async void PriestInputUpdate()
        {
            while (true)
            {
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                UpdateConnectionLabels(input.CheckConnections());
                input.Update();

                if (input.Master != con_master_combo.SelectedIndex)
                    if (input.Master == -1 && con_master_combo.SelectedIndex != 4)
                        con_master_combo.SelectedIndex = 4;
                    else if (input.Master != -1)
                        con_master_combo.SelectedIndex = input.Master;

            }
        }

        /// <summary>
        /// When user selects a new option in the combobox, this code will run checks on the selected option to make sure it's not invalid. 
        /// An invalid state is one where the user attempts to select as master the emulated controller or a port that has no controller selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MasterControllerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (input.Output != -1 && con_master_combo.SelectedIndex == input.Output)
            {
                MessageBox.Show(WarnOutputMaster, WarnErrorType, MessageBoxButton.OK, MessageBoxImage.Error);
                con_master_combo.SelectedIndex = 4;
            }
            else if (con_master_combo.SelectedIndex < 4 && !input.IsConnected(con_master_combo.SelectedIndex))
            {
                MessageBox.Show(WarnMasterDisconnect, WarnErrorType, MessageBoxButton.OK, MessageBoxImage.Error);
                con_master_combo.SelectedIndex = 4;
            }
            else
            {
                if (con_master_combo.SelectedIndex == 4)
                    input.Master = -1;
                else
                    input.Master = con_master_combo.SelectedIndex;
            }
        }
    }
}
