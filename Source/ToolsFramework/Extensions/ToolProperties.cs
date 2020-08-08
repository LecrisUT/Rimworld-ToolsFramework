﻿using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ToolsFramework
{
    public class ToolProperties : DefModExtension
    {
        public readonly List<ToolTypeModifier> toolTypesValue = new List<ToolTypeModifier>();
        public readonly List<JobModifier> jobBonus = new List<JobModifier>();
        public IEnumerable<ToolType> ToolTypes => toolTypesValue.Select(t => t.toolType);
    }
}
