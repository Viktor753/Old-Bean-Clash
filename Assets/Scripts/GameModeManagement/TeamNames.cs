using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TeamNames
{
    public static string teamOneName = "Blue Team";
    public static string teamTwoName = "Red Team";
    public static string undefinedTeamName = "No Team";
    public static string GetTeamName(int teamID)
    {
        switch (teamID)
        {
            case -1:
                return undefinedTeamName;
            case 0:
                return teamOneName;
            case 1:
                return teamTwoName;
        }

        return "NULL TEAM NAME";
    }
}
