using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CheckCuboid
{
    public class InputManager
    {
        Dictionary<string, InputHelper> mInputs = new Dictionary<string, InputHelper>();

        public MouseState CurrentMouseState;
        public MouseState PreviousMouseState;
        public MouseState OriginalMouseState;

        public InputManager()
        {
        }

        public InputHelper NewInput(string action)
        {
            if (mInputs.ContainsKey(action) == false)
            {
                mInputs.Add(action, new InputHelper());
            }

            return mInputs[action];
        }

        public void startUpdate()
        {
            CurrentMouseState = Mouse.GetState();
            InputHelper.startUpdate();
        }

        public void endUpdate()
        {
            PreviousMouseState = CurrentMouseState;
            InputHelper.endUpdate();
        }

        public void resetAllInput()
        {
            mInputs.Clear();
        }

        public bool IsPressed(string action, PlayerIndex? player)
        {
            if (mInputs.ContainsKey(action) == false)
                return false;

            return mInputs[action].IsPressed(player);
        }

        public bool[] GetMouseButtons()
        {
            bool[] _buttons = new bool[3];

            if (this.CurrentMouseState.LeftButton == ButtonState.Pressed)
                _buttons[0] = true;
            else
                _buttons[0] = false;

            if (this.CurrentMouseState.MiddleButton == ButtonState.Pressed)
                _buttons[1] = true;
            else
                _buttons[1] = false;

            if (this.CurrentMouseState.RightButton == ButtonState.Pressed)
                _buttons[2] = true;
            else
                _buttons[2] = false;

            return _buttons;
        }

        public void AddGamePadInput(string action, Buttons buttonPressed, bool isReleased)
        {
            NewInput(action).AddGamepadInput(buttonPressed, isReleased);
        }

        public void AddKeyboardInput(string action, Keys keyPressed, bool isReleased)
        {
            NewInput(action).AddKeyboardInput(keyPressed, isReleased);
        }
    }
}
