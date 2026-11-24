using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chastota885
{
    public enum GameParameter { Connection, Paranoia, Battery }
    public enum ConditionOperator { Greater, GreaterOrEqual, Equal, LessOrEqual, Less }

    [Serializable]
    public sealed class EffectData
    {
        public GameParameter parameter;
        public int amount;
    }

    [Serializable]
    public sealed class ParameterCondition
    {
        public GameParameter parameter;
        public ConditionOperator operation;
        public int value;

        public bool IsMet(GameState state)
        {
            int current = state.GetValue(parameter);
            switch (operation)
            {
                case ConditionOperator.Greater: return current > value;
                case ConditionOperator.GreaterOrEqual: return current >= value;
                case ConditionOperator.Equal: return current == value;
                case ConditionOperator.LessOrEqual: return current <= value;
                case ConditionOperator.Less: return current < value;
                default: return false;
            }
        }
    }

    [Serializable]
    public sealed class SignalCondition
    {
        public int minimumDay = 1;
        public int maximumDay = 999;
        public List<string> requiredFlags = new List<string>();
        public List<string> forbiddenFlags = new List<string>();
        public List<ParameterCondition> parameterConditions = new List<ParameterCondition>();

        public bool IsMet(GameState state)
        {
            if (state.CurrentDay < minimumDay || state.CurrentDay > maximumDay) return false;
            foreach (string flag in requiredFlags)
                if (!state.HasFlag(flag)) return false;
            foreach (string flag in forbiddenFlags)
                if (state.HasFlag(flag)) return false;
            foreach (ParameterCondition condition in parameterConditions)
                if (condition != null && !condition.IsMet(state)) return false;
            return true;
        }
    }

    [Serializable]
    public sealed class SignalData
    {
        public string id;
        public string title;
        [TextArea(3, 12)] public string body;
        public float frequency;
        public float tolerance = 0.05f;
        public string audioResourcePath;
        public SignalCondition availability = new SignalCondition();
        public List<ResponseData> responses = new List<ResponseData>();
    }

    [Serializable]
    public sealed class ResponseData
    {
        public string id;
        public string text;
        [Min(0)] public int batteryCost = 5;
        public List<EffectData> effects = new List<EffectData>();
        public List<string> flagsToAdd = new List<string>();
        public List<string> flagsToRemove = new List<string>();
        public string nextSignalId;
        public bool advanceDay = true;
    }

    [Serializable]
    public sealed class EndingCondition
    {
        public int priority;
        public string id;
        public string title;
        [TextArea(3, 12)] public string body;
        public List<string> requiredFlags = new List<string>();
        public List<string> forbiddenFlags = new List<string>();
        public List<ParameterCondition> parameterConditions = new List<ParameterCondition>();

        public bool IsMet(GameState state)
        {
            foreach (string flag in requiredFlags)
                if (!state.HasFlag(flag)) return false;
            foreach (string flag in forbiddenFlags)
                if (state.HasFlag(flag)) return false;
            foreach (ParameterCondition condition in parameterConditions)
                if (condition != null && !condition.IsMet(state)) return false;
            return true;
        }
    }

    [Serializable]
    public sealed class GameContent
    {
        public List<SignalData> signals = new List<SignalData>();
        public List<EndingCondition> endings = new List<EndingCondition>();
    }

    [Serializable]
    public sealed class SaveData
    {
        public int currentDay = 1;
        public int connection = 50;
        public int paranoia = 50;
        public int battery = 100;
        public bool hasAnsweredToday;
        public string currentSignalId;
        public string finalEndingId;
        public List<string> activeFlags = new List<string>();
        public List<string> completedSignalIds = new List<string>();
    }
}
