﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly
{
    /*
     * This is currently unused by the game
     * 
     * Here will be all AI actions and variables
     */
    class AI
    {
        public byte AI_ID;
        public byte[] morales = new byte[3] { 5, 5, 5 };
        public byte[] moraleTowardsPlayer = new byte[3] { 5, 5, 5 };

        public AI()//byte ID)
        {
            //AI_ID = ID;
        }

        public void SwitchFunctionAccordingToBoardPosition()
        {

        }
    }
}