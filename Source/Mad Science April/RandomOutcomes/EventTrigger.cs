using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BigAndSmall
{
    public class EventTrigger
    {
        public EventOutcome outcome;
        public float chance;

        public void LoadDataFromXmlCustom(System.Xml.XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, nameof(outcome), xmlRoot.Name);
            chance = float.TryParse(xmlRoot.FirstChild.Value, out float result) ? result : 0;
        }
    }
}
