using System.Xml;
using Verse;

namespace ToolsFramework
{
	public class BillGiverModifier
	{
		public ThingDef billGiver;
		public float value;
		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "billGiver", xmlRoot.Name);
			value = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
		}

		public override string ToString()
		{
			if (billGiver == null)
				return "(null billGiver)";
			return billGiver.defName + "-" + value.ToString();
		}
	}
}
