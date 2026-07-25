using System.Collections.Generic;

namespace EdgeAIKiosk1.Services;

public static class VerificationLabels
{
    public static HashSet<string> LabelsToVerify => KioskSettings.AcceptedLabels;
}
