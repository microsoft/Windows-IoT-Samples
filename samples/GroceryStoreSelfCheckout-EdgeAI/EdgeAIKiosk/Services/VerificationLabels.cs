using System.Collections.Generic;

namespace EdgeAIKiosk.Services;

public static class VerificationLabels
{
    public static HashSet<string> LabelsToVerify => KioskSettings.AcceptedLabels;
}
