using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace RecipeBrowser {
    /// <summary>
    /// A helper class to log errors while avoiding spamming the log with the same error.
    /// </summary>
    public static class RecurrentErrorLogger {
        /// <summary>
        /// Stores log data for a specific error message. This is used to center data in a thread-safe manner.
        /// </summary>
        private class LogData {
            /// <summary>
            /// A queue of timestamps for each time the logger was called for this message.
            /// </summary>
            public Queue<DateTime> Timestamps { get; } = new Queue<DateTime>();

            /// <summary>
            /// The total number of times the logger actually logged the message.
            /// </summary>
            public int TotalCount { get; set; } = 0;
        }
        
        private static Mod mod;

        // TODO: This could be a memory leak if the dictionary grows indefinitely. Consider adding a limit to the number of messages stored or a time limit.
        /// <summary>
        /// Stores the log data for each error message that has been logged.
        /// </summary>
        private static readonly ConcurrentDictionary<string, LogData> LogRecords = new ConcurrentDictionary<string, LogData>();

        // TODO: `MaxLogsPerMessage`, `MaxMessagesWithinInterval` and `Interval` could be configurable. Maybe add them as configs to the mod.
        /// <summary>
        /// The maximum number of times that can be logged for a single message.
        /// </summary>
        private const int MaxLogsPerMessage = 5;
        /// <summary>
        /// The maximum number of times that an error can happen within a specific interval before it is logged.
        /// </summary>
        private const int MaxMessagesWithinInterval = 10;
        /// <summary>
        /// The interval in which the error can recur before it is logged.
        /// </summary>
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

        /// <summary>
        /// A lock object to ensure that the log data is accessed in a thread-safe manner.
        /// </summary>
        private static readonly object lockObject = new object();

        /// <summary>
        /// Logs an error message if the error happens more than a certain number of times within a specific interval.
        /// To avoid spamming the log, the error is only logged a maximum number of times.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        public static void MaybeLog(string message) {
            if (mod == null) {
                mod = ModContent.GetInstance<RecipeBrowser>();
            }

            LogData data = LogRecords.GetOrAdd(message, _ => new LogData());
            lock (lockObject) {
                if (data.TotalCount >= MaxLogsPerMessage) {
                    return;
                }

                data.Timestamps.Enqueue(DateTime.Now);

                if (data.Timestamps.Count > MaxMessagesWithinInterval && IsWithinInterval(data.Timestamps.Peek())) {
                    mod.Logger.Error($"Error happened more than {MaxMessagesWithinInterval} times within {Interval.TotalSeconds} seconds: {message}");

                    data.TotalCount++;
                    data.Timestamps.Clear();
                }

                if (data.TotalCount >= MaxLogsPerMessage) {
                    mod.Logger.Error($"Error logged more than {MaxLogsPerMessage} times, suppressing further logs");
                }
            }
        }

        /// <summary>
        /// Checks if the given timestamp is within the interval.
        /// </summary>
        /// <param name="timestamp">The timestamp to check.</param>
        /// <returns><see langword="true"/> if the timestamp is within the interval; otherwise, <see langword="false"/>.</returns>
        private static bool IsWithinInterval(DateTime timestamp) {
            return DateTime.Now - timestamp <= Interval;
        }
    }
}