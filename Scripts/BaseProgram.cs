/*-*/
// This file was derived from
// http://forum.keenswh.com/threads/guide-setting-up-visual-studio-for-programmable-block-scripting.7225319/
// Huge credit to them for getting the basic information I needed

/* Requires references to (<SE Install> = space engineers install)
 * <SE Install>\Sandbox.Common.dll
 * <SE Install>\Sandbox.Game.dll
 * <SE Install>\SpaceEngineers.Game.dll
 * <SE Install>\VRage.dll
 * <SE Install>\VRage.Game.dll
 * <SE Install>\VRage.Library.dll
 * <SE Install>\VRage.Math.dll
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
// using Sandbox.Common.Components // I belive this has moved to VRage.Game.Components (VS will automatically find the missing references anyway)
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.ModAPI.Ingame;
// Some definitions are in VRage.Game.ModAPI.Ingame so beware
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;

namespace Scripts.BaseProgram // change 'BaseProgram' to the name of the script itself to stop accidental cross-referencing
{
    class Program : Sandbox.ModAPI.IMyGridProgram // class name needs to be 'Program' in order to satisfy constructor method
    {
        // These are members implemented using NotImplementedException
        // in order to satisfy VS in extending the Sandbox.ModAPI.IMyGridProgram
        // interface (which is where all programming block programs extend from)
        public Action<string> Echo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public TimeSpan ElapsedTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IMyGridTerminalSystem GridTerminalSystem { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool HasMainMethod => throw new NotImplementedException();
        public bool HasSaveMethod => throw new NotImplementedException();
        public IMyProgrammableBlock Me { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IMyGridProgramRuntimeInfo Runtime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Storage { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        //public void Save() => throw new NotImplementedException();
        //public void Main(string argument) => throw new NotImplementedException();
        //public void Main(string argument, UpdateType updateSource) => throw new NotImplementedException();

        /* ===== Additional info =====
        * public string IMyTerminalBlock.CustomData -> synced and saved custom data that can be edited via terminal
        * 
        * Programs extend from
        *   Sandbox.ModAPI.IMyGridProgram
        * 
        * Solar power output contained in
        *   IMyTerminalBlock.DetailedInfo
        * 
        * Actions can be triggered via
        *   IMyTerminalBlock.ApplyAction(string action)
        * 
        * Use 'Right Click -> Peek Definition' to see what you can do with an inferface (the documentation is lacking to say the least)
        * 
        * The following is an extention class the IMyTerminalBlock interface
        *   Sandbox.ModAPI.Ingame.TerminalBlockExtentions
        * 
        * Runtime.UpdateFrequency allows you to automate the running of the script without using timer
        *   blocks. If you are monitoring something (or doing something similar that requires repeated calls)
        *   then use this rather than a timer block.
        */

        /*-*/
        public Program()
        {

            /* The constructor, called only once every session and
             * always before any other method is called. Use it to
             * initialize your script.
             * 
             * The constructor is optional and can be removed if not
             * needed.
             */

        }

        public void Save()
        {

            /* Called when the program needs to save its state. Use
             * this method to save your state to the Storage field
             * or some other means. 
             * 
             * This method is optional and can be removed if not
             * needed.
             * 
             * if you dont need this method, you can remove it and
             * uncomment the line in the NotImplementedException
             * methods.
             */

        }

        public void Main(string argument)
        {

            /* The main entry point of the script, invoked every time
             * one of the programmable block's Run actions are invoked.
             * 
             * The method itself is required, but the argument above
             * can be removed if not needed.
             */

        }

        public void Main(string argument, UpdateType updateSource)
        {
            /* An alternative entry point of the script.
             * It does the same as the other Main method, however
             * it give you the 'updateSource' variable, which allows
             * you to distingush between manuage invokes and invokes via
             * Runtime.UpdateFrequency
             */
        }

        /*-*/
    }
}
/*-*/
