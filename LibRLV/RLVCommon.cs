using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibRLV
{
    public static class RLVCommon
    {
        public static List<object> ParseOptions(string options)
        {
            var result = new List<object>();
            var args = options.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var arg in args)
            {
                if (UUID.TryParse(arg, out var id))
                {
                    result.Add(id);
                    continue;
                }
                else if (int.TryParse(arg, out var intValue))
                {
                    result.Add(intValue);
                    continue;
                }
                else if (float.TryParse(arg, out var floatValue))
                {
                    result.Add(floatValue);
                    continue;
                }
                else if (Enum.TryParse(arg, true, out WearableType part) && part != WearableType.Invalid)
                {
                    result.Add(part);
                    continue;
                }
                else if (Enum.TryParse(arg, true, out AttachmentPoint attachmentPoint))
                {
                    result.Add(attachmentPoint);
                    continue;
                }
                else
                {
                    result.Add(arg);
                }
            }

            return result;
        }
    }
}
