using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using JetBrains.Annotations;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using SharpDX.XInput;

namespace ControllerPriestGUI.Logic
{
    class PriestInput
    {
        private Controller[] controllers;

        private int master = -1;
        private int output = -1;
        private int lastPacketNum = -1;
        private bool allowTakeControl = false;
        
        private ViGEmClient client;
        private IXbox360Controller xController;
        private Gamepad masterSwitchState;

        public int Output { get => output; set => output = value; }
        public int Master { get => master; set => master = value; }
        public bool TakeControl { get => allowTakeControl; set => allowTakeControl = value; }

        /// <summary>
        /// Startup PriestInput class.
        /// </summary>
        public PriestInput()
        {
            client = new ViGEmClient();
            xController = client.CreateXbox360Controller();
            controllers = new[] { new Controller(UserIndex.One), new Controller(UserIndex.Two), new Controller(UserIndex.Three), new Controller(UserIndex.Four) };

            masterSwitchState.Buttons = (GamepadButtonFlags.LeftShoulder | GamepadButtonFlags.RightShoulder | GamepadButtonFlags.RightThumb | GamepadButtonFlags.LeftThumb);
        }

        /// <summary>
        /// Run update loop to pass controller state to the output controller. As well as check if new master command has been triggered.
        /// </summary>
        public void Update()
        {
            if (output != -1 && master != -1)
            {
                if (master != -1 && controllers[master].IsConnected && controllers[master].GetState().PacketNumber != lastPacketNum)
                {
                    State currState = controllers[master].GetState();
                    lastPacketNum = currState.PacketNumber;

                    if (ChangeMasterTriggered(currState))
                    {
                        ChangeMaster();
                    }
                    else
                    {

                        //TODO: Is there a better way of doing this? Am I being stupid? I feel like I am.
                        // It would be nice if we had direct access to pass straight to XInput.h state var, like I could do in C++.
                        xController.SetButtonState(Xbox360Button.A, currState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A));
                        xController.SetButtonState(Xbox360Button.B, currState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B));
                        xController.SetButtonState(Xbox360Button.X, currState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.X));
                        xController.SetButtonState(Xbox360Button.Y, currState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Y));

                        xController.SetButtonState(Xbox360Button.Up, currState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp));
                        xController.SetButtonState(Xbox360Button.Down, currState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown));
                        xController.SetButtonState(Xbox360Button.Left, currState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft));
                        xController.SetButtonState(Xbox360Button.Right, currState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight));

                        xController.SetButtonState(Xbox360Button.Start, currState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Start));
                        xController.SetButtonState(Xbox360Button.Back, currState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Back));

                        xController.SetButtonState(Xbox360Button.RightThumb, currState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightThumb));
                        xController.SetButtonState(Xbox360Button.LeftThumb, currState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftThumb));

                        xController.SetButtonState(Xbox360Button.LeftShoulder, currState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder));
                        xController.SetButtonState(Xbox360Button.RightShoulder, currState.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder));

                        xController.SetAxisValue(Xbox360Axis.LeftThumbX, currState.Gamepad.LeftThumbX);
                        xController.SetAxisValue(Xbox360Axis.LeftThumbY, currState.Gamepad.LeftThumbY);
                        xController.SetAxisValue(Xbox360Axis.RightThumbX, currState.Gamepad.RightThumbX);
                        xController.SetAxisValue(Xbox360Axis.RightThumbY, currState.Gamepad.RightThumbY);
                        xController.SetSliderValue(Xbox360Slider.LeftTrigger, currState.Gamepad.LeftTrigger);
                        xController.SetSliderValue(Xbox360Slider.RightTrigger, currState.Gamepad.RightTrigger);

                        xController.SubmitReport();
                    }

                }
                else if (master != -1 && !controllers[master].IsConnected)
                {
                    ChangeMaster();

                    if (master != -1 && !controllers[master].IsConnected)
                    {
                        master = -1;
                    }
                }
            }
            
            if ((output != -1 && master == -1) || allowTakeControl)
            {
                //If no master has been set, but output is set we should see if any controller wants to manually grab control.
                for (int i = 0; i < controllers.Length; i++)
                {
                    if (i != output)
                    {
                        if (controllers[i].IsConnected)
                        {
                            if (ChangeMasterTriggered(controllers[i].GetState()))
                            {
                                master = i;
                            }
                        }    
                    }
                }
            }
        }

        /// <summary>
        /// Try to find a new master controller that isn't the output controller. If no other potential master is found,
        /// then 
        /// </summary>
        public void ChangeMaster()
        {
            for (int i = master + 1; i != master; i++)
            {
                if (i >= 4)
                    i = 0;

                if (controllers[i].IsConnected && i != output)
                {
                    master = i;
                    return;
                }
            }
        }

        /// <summary>
        /// Check what ports have controllers connected to it.
        /// </summary>
        /// <returns>bool array showing the status of the controller ports. True = Connected | False = Disconnected</returns>
        public bool[] CheckConnections()
        {
            var results = new[] { false, false, false, false };

            for (int i = 0; i < controllers.Length; i++)
            {
                if (controllers != null)
                {
                    results[i] = controllers[i].IsConnected;
                }
            }

            return results;
        }

        /// <summary>
        /// Allow an external class to start the emulated controller.
        /// </summary>
        public void StartOutputController()
        {
            xController.Connect();
        }

        /// <summary>
        /// Allow external class to stop the emulated controller.
        /// </summary>
        public void StopOutputController()
        {
            xController.Disconnect();
        }

        /// <summary>
        /// Allow external class to check if a controller is connected.
        /// </summary>
        /// <param name="index">The controller port number (ports start at 0)</param>
        /// <returns></returns>
        public bool IsConnected(int index)
        {
            return controllers[index].IsConnected;
        }

        /// <summary>
        /// Check to see if the state of a controller has requested a change in master.
        /// </summary>
        /// <param name="state">The current state of a controller</param>
        /// <returns></returns>
        public bool ChangeMasterTriggered(State state)
        {
            return (state.Gamepad.Buttons == masterSwitchState.Buttons);
        }

    }
}
