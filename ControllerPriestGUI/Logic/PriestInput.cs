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
using ControllerPriest.Logic;

namespace ControllerPriestGUI.Logic
{
    class PriestInput
    {
        private Controller[] controllers;
        private ControllerConfig control_config;
        private Dictionary<Xbox360Button, GamepadButtonFlags> player_mapping_2;
        private Dictionary<Xbox360Button, GamepadButtonFlags> default_mapping;

        private int master = -1;
        private int output = -1;
        private int lastPacketNum = -1;
        private bool allowTakeControl = false;
        
        private ViGEmClient client;
        private IXbox360Controller xController;
        //Button combination to switch controller.
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
            controllers = new[] { new Controller(UserIndex.One), new Controller(UserIndex.Two), new Controller(UserIndex.Three), new Controller(UserIndex.Four) };

            masterSwitchState.Buttons = (GamepadButtonFlags.LeftShoulder | GamepadButtonFlags.RightShoulder | GamepadButtonFlags.RightThumb | GamepadButtonFlags.LeftThumb);
            control_config = new ControllerConfig(ref client);
        }

        /// <summary>
        /// Run update loop to pass controller state to the output controller. As well as check if new master command has been triggered.
        /// </summary>
        public void Update()
        {
            if (output != -1 && master != -1)
            {
                if (controllers[master].IsConnected && controllers[master].GetState().PacketNumber != lastPacketNum)
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
                        foreach (KeyValuePair<Xbox360Button, GamepadButtonFlags> entry in control_config.GetControllerMapping())
                        {
                            control_config.GetController().SetButtonState(entry.Key, currState.Gamepad.Buttons.HasFlag(entry.Value));
                        }

                        control_config.SetAxisValue(Xbox360Axis.LeftThumbX, currState.Gamepad.LeftThumbX);
                        control_config.SetAxisValue(Xbox360Axis.LeftThumbY, currState.Gamepad.LeftThumbY);
                        control_config.SetAxisValue(Xbox360Axis.RightThumbX, currState.Gamepad.RightThumbX);
                        control_config.SetAxisValue(Xbox360Axis.RightThumbY, currState.Gamepad.RightThumbY);
                        control_config.SetSliderValue(Xbox360Slider.LeftTrigger, currState.Gamepad.LeftTrigger);
                        control_config.SetSliderValue(Xbox360Slider.RightTrigger, currState.Gamepad.RightTrigger);

                        control_config.GetController().SubmitReport();
                    }

                }
                else if (!controllers[master].IsConnected)
                {
                    ChangeMaster();

                    //We check to see if ChangeMaster, succeeded. If it didn't, we can assume no controllers are currently connected.
                    if (!controllers[master].IsConnected)
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
            //Go one port up from current master, and loop to find connected controller.
            for (int i = master + 1; i != master; i++)
            {
                //If we are over maxmimum number of ports, wrap number back to 0.
                if (i >= 4)
                    i = 0;

                if (controllers[i].IsConnected && i != output)
                {
                    master = i;
                    return;
                }
            }
        }
        
        public Dictionary<Xbox360Button, GamepadButtonFlags> GetDefaultButtonMappings()
        {
            Dictionary<Xbox360Button, GamepadButtonFlags> result = new Dictionary<Xbox360Button, GamepadButtonFlags>();

            result.Add(Xbox360Button.A, GamepadButtonFlags.A);
            result.Add(Xbox360Button.B, GamepadButtonFlags.B);
            result.Add(Xbox360Button.X, GamepadButtonFlags.X);
            result.Add(Xbox360Button.Y, GamepadButtonFlags.Y);

            result.Add(Xbox360Button.Up, GamepadButtonFlags.DPadUp);
            result.Add(Xbox360Button.Down, GamepadButtonFlags.DPadDown);
            result.Add(Xbox360Button.Left, GamepadButtonFlags.DPadLeft);
            result.Add(Xbox360Button.Right, GamepadButtonFlags.DPadRight);

            result.Add(Xbox360Button.Start, GamepadButtonFlags.Start);
            result.Add(Xbox360Button.Back, GamepadButtonFlags.Back);

            result.Add(Xbox360Button.RightThumb, GamepadButtonFlags.RightThumb);
            result.Add(Xbox360Button.LeftThumb, GamepadButtonFlags.LeftThumb);

            result.Add(Xbox360Button.LeftShoulder, GamepadButtonFlags.LeftShoulder);
            result.Add(Xbox360Button.RightShoulder, GamepadButtonFlags.RightShoulder);

            return result;
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
            control_config.GetController().Connect();
        }

        /// <summary>
        /// Allow external class to stop the emulated controller.
        /// </summary>
        public void StopOutputController()
        {
            control_config.GetController().Disconnect();
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

        public void CurseControls()
        {
            control_config.CurseController();
        }

        public bool IsControlCursed()
        {
            return control_config.is_cursed;
        }
    }
}
