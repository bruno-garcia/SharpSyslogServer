namespace SharpSyslogServer.SyslogMessageFormat
{
    /// <summary>
    /// Message Facility
    /// </summary>
    /// <remarks>
    /// Facility values MUST be in the range of 0 to 23 inclusive.
    /// https://tools.ietf.org/html/rfc5424#section-6.2.1
    /// </remarks>
    public enum Facility
    {
        KernelMessages = 0,
        UserLevelMessages = 1,
        MailSystem = 2,
        SystemDaemons = 3,
        SecurityAuthorizationMessages4 = 4,
        MessagesGeneratedInternallyBySyslogd = 5,
        LinePrinterSubsystem = 6,
        NetworkNewsSubsystem = 7,
        UucpSubsystem = 8,
        ClockDaemon = 9,
        SecurityAuthorizationMessages10 = 10,
        FtpDaemon = 11,
        NtpSubsystem = 12,
        LogAudit = 13,
        LogAlert = 14,
        ClockDaemonNote2 = 15,
        LocalUse0 = 16,
        LocalUse1 = 17,
        LocalUse2 = 18,
        LocalUse3 = 19,
        LocalUse4 = 20,
        LocalUse5 = 21,
        LocalUse6 = 22,
        LocalUse7 = 23,
    }
}