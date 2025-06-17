using System.ComponentModel;
using System.Reflection;

namespace TMS.Repository.Enums;

public static class EnumHelper
{
    public static string ToDescription(this Enum enumeration)
    {
        Type type = enumeration.GetType();
        MemberInfo[] memInfo = type.GetMember(enumeration.ToString());

        if (null != memInfo && memInfo.Length > 0)
        {
            object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (null != attrs && attrs.Length > 0)
                return ((DescriptionAttribute)attrs[0]).Description;
        }

        return enumeration.ToString();
    }
}
