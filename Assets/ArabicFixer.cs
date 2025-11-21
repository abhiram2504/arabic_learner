using UnityEngine;
using System.Text;
using RTLTMPro;

public static class ArabicFixer
{
    public static string Fix(string input)
    {
        return RTLTMPro.RTLTextMeshPro.FixRTL(input, false, false);
    }
}