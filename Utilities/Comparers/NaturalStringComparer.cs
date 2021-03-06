﻿using System;
using System.Collections.Generic;

namespace Utilities.Comparers
{
    /// <summary>Compares two strings based on their natural sort order (<see cref="http://en.wikipedia.org/wiki/Natural_sort_order"/>) instead of ASCII code order.</summary>
    public class NaturalStringComparer : IComparer<string>
    {
        public static NaturalStringComparer OrdinalIgnoreCase { get; } = new NaturalStringComparer(true);
        public static NaturalStringComparer Ordinal { get; } = new NaturalStringComparer(false);

        private bool IgnoreCase { get; }

        private NaturalStringComparer(bool ignoreCase)
        {
            IgnoreCase = ignoreCase;
        }

        private static readonly char[] Digits = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
        private const char CHAR_NULL = '\0';

        /// <inheritdoc/>
        public int Compare(string x, string y) => CompareOrdinal(x, y, IgnoreCase);

        /// <inheritdoc/>
        public static int CompareOrdinal(string x, string y, bool ignoreCase)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            var shorterLength = Math.Min(x.Length, y.Length);

            for (int i = 0; i < shorterLength; i++)
            {
                // If both characters are digits we need to sort based on the entire integer value, not individual code point comparisons.
                if (x[i] <= 0x39 && y[i] <= 0x39 && 0x30 <= x[i] && 0x30 <= y[i])
                {
                    char yFirstDifferentDigit;
                    char xFirstDifferentDigit;

                    if (x[i] == y[i])
                    {
                        // Denote that every pair of numbers seen so far has been the same
                        xFirstDifferentDigit = CHAR_NULL;
                        yFirstDifferentDigit = CHAR_NULL;
                    }
                    else
                    {
                        xFirstDifferentDigit = x[i];
                        yFirstDifferentDigit = y[i];
                    }

                    // Continue reading until one string stops being digits
                    for (i++;; i++)
                    {
                        bool xHasDigit = i < x.Length && x[i] <= 0x39 && 0x30 <= x[i];
                        bool yHasDigit = i < y.Length && y[i] <= 0x39 && 0x30 <= y[i];

                        if (xHasDigit && yHasDigit)
                        {
                            // If they are are both digits we need to continue reading characterss
                            if (xFirstDifferentDigit == CHAR_NULL && x[i] != y[i])
                            {
                                // they are different numbers, so now we just need to find out which one is larger
                                xFirstDifferentDigit = x[i];
                                yFirstDifferentDigit = y[i];
                            }
                        }
                        else if (!xHasDigit && !yHasDigit)
                        {
                            // We stopped reading in digits at the same time so the smaller number is the determined by the first digit that is different between them 
                            if (xFirstDifferentDigit != CHAR_NULL) return xFirstDifferentDigit - yFirstDifferentDigit;

                            // Both strings contained the same integer so return to alphabetical comparisonss
                            break;
                        }
                        else
                        {
                            // The smaller number is the one with less digits
                            return yHasDigit ? -1 : 1;
                        }
                    }

                    if (i >= shorterLength) break;
                }

                var ordinalComparison = x[i].CompareTo(y[i]);
                if (ordinalComparison != 0)
                {
                    if (!ignoreCase) return ordinalComparison;

                    // Check for case insensitive equality - this is not done initially due to the relatively high cost of ToLowerInvariant and the expectation that most comparisons will not be between two characters that only vary in case.
                    // ToLowerInvariant is used instead of the standard ToUpperInvariant so that the characters [\]^_` appear before letters
                    int ordinalCaseInsensitiveComparison = char.ToLowerInvariant(x[i]).CompareTo(char.ToLowerInvariant(y[i]));
                    if (ordinalCaseInsensitiveComparison != 0) return ordinalCaseInsensitiveComparison;
                }
            }

            return x.Length.CompareTo(y.Length);
        }
    }
}
