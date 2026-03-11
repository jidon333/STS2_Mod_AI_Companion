using System.Collections.Generic;
using System.Linq;

namespace MegaCrit.Sts2.Core.HoverTips;

public static class HoverTipExtensions
{
	public static void MegaTryAddingTip(this ICollection<IHoverTip> tips, IHoverTip tip)
	{
		IHoverTip tip2 = tip;
		IHoverTip hoverTip = tips.FirstOrDefault((IHoverTip t) => t.Id == tip2.Id);
		if (hoverTip != null && !hoverTip.IsInstanced)
		{
			if (!hoverTip.IsSmart && tip2.IsSmart)
			{
				tips.Remove(hoverTip);
				tips.Add(tip2);
			}
		}
		else
		{
			tips.Add(tip2);
		}
	}
}
