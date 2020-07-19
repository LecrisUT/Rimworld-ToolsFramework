using System.Xml;
using Verse;

namespace ToolsFramework
{
	public class JobModifier
	{
		public JobDef job;
		public float value;
		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "JobDef", xmlRoot.Name);
			value = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
		}

		public override string ToString()
		{
			if (job == null)
			{
				return "(null JobDef)";
			}
			return job.defName + "-" + value.ToString();
		}
	}
}
