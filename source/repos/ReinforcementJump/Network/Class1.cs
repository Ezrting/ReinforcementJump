using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReinforcementJump
{
    public class DataPoint
    {
        public double[] Inputs { get; set; }
        public double[] ExpectedOutputs { get; set; }

        public string StringFormOutputs { get; set; }
        public float Strength = 1;
        public int ChosenAction;
        // required modifier means that the field must be set through initialization
        public DataPoint(double[] inputs)
        {
            Inputs = inputs;
          //  (ExpectedOutputs, StringFormOutputs) = RunInputs(inputs);

        }
    }
}
