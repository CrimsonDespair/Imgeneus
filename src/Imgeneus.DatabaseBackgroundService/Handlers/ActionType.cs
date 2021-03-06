﻿namespace Imgeneus.DatabaseBackgroundService.Handlers
{
    /// <summary>
    /// All possible actions to database.
    /// </summary>
    public enum ActionType
    {
        // Character
        SAVE_CHARACTER_MOVE,
        SAVE_CHARACTER_HP_MP_SP,

        // Inventory
        SAVE_ITEM_IN_INVENTORY,
        REMOVE_ITEM_FROM_INVENTORY,
        UPDATE_ITEM_COUNT_IN_INVENTORY,
        UPDATE_GOLD,
        CREATE_DYE_COLOR,
        UPDATE_CRAFT_NAME,

        // Gems
        UPDATE_GEM,

        // Stats
        UPDATE_STATS,

        // Skills
        SAVE_SKILL,
        REMOVE_SKILL,

        // Buffs
        SAVE_BUFF,
        REMOVE_BUFF,
        REMOVE_BUFF_ALL,
        UPDATE_BUFF_RESET_TIME,

        // Quests
        QUEST_START,
        QUEST_UPDATE,

        // Appearance
        SAVE_APPEARANCE,

        // Friends
        SAVE_FRIENDS,
        DELETE_FRIENDS,

        // Map
        SAVE_MAP_ID,

        // Logs
        LOG_SAVE_CHAT_MESSAGE
    }
}
