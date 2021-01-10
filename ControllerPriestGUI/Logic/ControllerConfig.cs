using Nefarius.ViGEm.Client.Targets.Xbox360;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Text;

namespace ControllerPriest.Logic
{
    class ControllerConfig
    {
        static Dictionary<Xbox360Button, GamepadButtonFlags> default_mapping;
        static List<GamepadButtonFlags> default_list;
        private Dictionary<Xbox360Button, GamepadButtonFlags> controller_mapping;
        private Dictionary<Xbox360Axis, bool> axis_config;
        private IXbox360Controller x_controller;
        private List<GamepadButtonFlags> cursed_list;
        private bool swap_triggers = false;
        private static Random rng = new Random();
        public bool is_cursed = false;

        public ControllerConfig(ref ViGEmClient client)
        {
            if (ControllerConfig.default_mapping == null)
            {
                ControllerConfig.SetDefaultMapping();
                ControllerConfig.SetDefaultList();
                
            }

            controller_mapping = new Dictionary<Xbox360Button, GamepadButtonFlags>(default_mapping);
            x_controller = client.CreateXbox360Controller();
            cursed_list = new List<GamepadButtonFlags>();
            
            //Initial setup of list to allow randomisation.
            foreach (KeyValuePair<Xbox360Button, GamepadButtonFlags> entry in controller_mapping)
            {
                cursed_list.Add(entry.Value);
            }

            axis_config = new Dictionary<Xbox360Axis, bool>();
            axis_config.Add(Xbox360Axis.LeftThumbX, false);
            axis_config.Add(Xbox360Axis.LeftThumbY, false);
            axis_config.Add(Xbox360Axis.RightThumbX, false);
            axis_config.Add(Xbox360Axis.RightThumbY, false);
        }

        public void Update()
        {
            return;
        }

        public void SetAxisValue(Xbox360Axis axis, short value)
        {
            if (axis_config[axis])
            {
                //TODO: Edge case could cause problems. Best be careful!
                x_controller.SetAxisValue(axis, (short) ~value);
            }
            else
            {
                x_controller.SetAxisValue(axis, value);
            }
        }

        public void SetSliderValue(Xbox360Slider slider, byte value)
        {
            if (swap_triggers)
                slider = (slider == Xbox360Slider.LeftTrigger ? Xbox360Slider.RightTrigger : Xbox360Slider.LeftTrigger);

            x_controller.SetSliderValue(slider, value);
        }

        public Dictionary<Xbox360Button, GamepadButtonFlags> GetControllerMapping()
        {
            return controller_mapping;
        }

        public ref IXbox360Controller GetController()
        {
            return ref x_controller;
        }

        public void CurseController()
        {
            if (!is_cursed)
            {
                Shuffle(ref cursed_list);

                int i = 0;
                foreach (KeyValuePair<Xbox360Button, GamepadButtonFlags> entry in default_mapping)
                {
                    controller_mapping[entry.Key] = cursed_list[i];
                    i++;
                }

                if (rng.Next(100) > 69)
                {
                    swap_triggers = true;
                }

                foreach (Xbox360Axis axis in new List<Xbox360Axis>(axis_config.Keys))
                {
                    axis_config[axis] = false;
                    if (rng.Next(100) > 49)
                    {
                        axis_config[axis] = true;
                    }
                }

                is_cursed = true;
            }
            else
            {
                controller_mapping = new Dictionary<Xbox360Button, GamepadButtonFlags>(default_mapping);
                swap_triggers = false;
                foreach (Xbox360Axis axis in new List<Xbox360Axis>(axis_config.Keys))
                {
                    axis_config[axis] = false;
                }

                is_cursed = false;
            }

            
        }

        static void SetDefaultMapping()
        {
            ControllerConfig.default_mapping = new Dictionary<Xbox360Button, GamepadButtonFlags>();
            ControllerConfig.default_mapping.Add(Xbox360Button.A, GamepadButtonFlags.A);
            ControllerConfig.default_mapping.Add(Xbox360Button.B, GamepadButtonFlags.B);
            ControllerConfig.default_mapping.Add(Xbox360Button.X, GamepadButtonFlags.X);
            ControllerConfig.default_mapping.Add(Xbox360Button.Y, GamepadButtonFlags.Y);

            ControllerConfig.default_mapping.Add(Xbox360Button.Up, GamepadButtonFlags.DPadUp);
            ControllerConfig.default_mapping.Add(Xbox360Button.Down, GamepadButtonFlags.DPadDown);
            ControllerConfig.default_mapping.Add(Xbox360Button.Left, GamepadButtonFlags.DPadLeft);
            ControllerConfig.default_mapping.Add(Xbox360Button.Right, GamepadButtonFlags.DPadRight);

            ControllerConfig.default_mapping.Add(Xbox360Button.Start, GamepadButtonFlags.Start);
            ControllerConfig.default_mapping.Add(Xbox360Button.Back, GamepadButtonFlags.Back);

            ControllerConfig.default_mapping.Add(Xbox360Button.RightThumb, GamepadButtonFlags.RightThumb);
            ControllerConfig.default_mapping.Add(Xbox360Button.LeftThumb, GamepadButtonFlags.LeftThumb);

            ControllerConfig.default_mapping.Add(Xbox360Button.LeftShoulder, GamepadButtonFlags.LeftShoulder);
            ControllerConfig.default_mapping.Add(Xbox360Button.RightShoulder, GamepadButtonFlags.RightShoulder);
        }

        static void SetDefaultList()
        {
            default_list = new List<GamepadButtonFlags>();
            foreach (KeyValuePair<Xbox360Button, GamepadButtonFlags> entry in default_mapping)
            {
                default_list.Add(entry.Value);
            }
        }

        static Dictionary<Xbox360Button, GamepadButtonFlags> GetDefaultMapping()
        {
            return default_mapping;
        }
        
        void Shuffle<T>(ref List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
