using System;
using System.Collections.Generic;
using System.Text;

namespace My_money.Templates
{
    public static class EmailTemplates
    {
        public static string PasswordReset(string tempPassword)
        {
            return $"""
        You have requested to reset your password.

        Temporary password:
        {tempPassword}

        Please log in using this password and set a new one immediately.

        If you did not request a password reset, you can safely ignore this email.

        Support Team myMoney app : st67130@upce.cz
        """;
        }

        public static string NewUser(string tempPassword)
        {
            return $"""
        A new account has been created for you.

        Temporary password:
        {tempPassword}

        Please log in and change your password as soon as possible.

        For security reasons, do not share this password with anyone.

        If you did not expect this account, please ignore this email or contact support.

        Support Team myMoney app : st67130@upce.cz
        """;
        }

    }
}
