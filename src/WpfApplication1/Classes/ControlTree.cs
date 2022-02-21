using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;

namespace AssetBuilder.Classes
{
	public static class ControlTree
	{
		public static IEnumerable<type> getChildren<type>(DependencyObject dp)
		{
			if (dp != null)
			{
				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dp); i++)
				{
					object p = VisualTreeHelper.GetChild(dp, i);
					if (p == null || p is type)
					{
						yield return (type)p;
					}

					foreach (type item in getChildren<type>(p as DependencyObject))
					{
						yield return item;
					}
				}
			}
		}

        public static type getChild<type>(DependencyObject dp)
        {
            if (dp == null) return default(type);
            type ret = default(type);

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dp); i++)
            {
                object p = VisualTreeHelper.GetChild(dp, i);
                if (p == null || p is type)
                {
                    ret = (type)p;
                    break;
                }
                else ret = getChild<type>(p as DependencyObject);
            }
            return ret;
        }

        public static type getLogicalChild<type>(DependencyObject dp)
        {
            if (dp == null) return default(type);
            type ret = default(type);

            foreach(var p in LogicalTreeHelper.GetChildren(dp))
            {
                if (p == null || p is type)
                {
                    ret = (type)p;
                    break;
                }
                else ret = getLogicalChild<type>(p as DependencyObject);
            }
            return ret;
        }

        public static type getParent<type>(DependencyObject dp)
		{
			if (dp == null) return default(type);
			type ret = default(type);

			object p = VisualTreeHelper.GetParent(dp);
			if (p == null || p is type)
			{
				ret = (type)p;
			}
			else ret = getParent<type>(p as DependencyObject);
			return ret;
		}
	}
}
