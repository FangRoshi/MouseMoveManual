using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouseMoveManual
{
    public class AnimaBean
    {
        public AnimaBean() { }
        public AnimaBean(int id,string nameAnimation,int feelStep,bool isFaintness,float start = 0f, float mid = 0.65f,float forward = 1.0f)
        {
            this.Id = id;
            this.nameAnimation = nameAnimation;
            this.feelStep = feelStep;
            this.isFaintness = isFaintness;
            this.StartProgress = start;
            this.MidProgress = mid;
            this.TProgress = start;
            this.ForwardProgress = forward;
            this.Multiple = 2f;

        }
        public int Id { get; set; }
        public string nameAnimation { get; set; }
        public bool isFaintness { get; set; }

        public int feelStep { get; set; }
        public float StartProgress { get; set; }
        public float MidProgress { get; set; }

        public float TProgress { get; set; }

        public float ForwardProgress { get; set; }

        public float Multiple { get; set; }

    }
}
