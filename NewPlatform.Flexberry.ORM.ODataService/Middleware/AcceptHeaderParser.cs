#if NETSTANDARD

// Основная часть кода взята из:
//
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//     Licensed under the MIT License.  See License.txt in the project root for license information.
//     https://github.com/OData/odata.net/blob/7.5.0/src/Microsoft.OData.Client/ContentTypeUtil.cs
//
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//     Licensed under the MIT License.  See License.txt in the project root for license information.
//     https://github.com/OData/odata.net/blob/7.5.0/test/FunctionalTests/Service/Microsoft/OData/Service/Error.cs
//
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//     Licensed under the MIT License.  See License.txt in the project root for license information.
//     https://github.com/OData/odata.net/blob/7.5.0/src/Microsoft.OData.Client/Build.Portable/Parameterized.Microsoft.OData.Client.Portable.cs
//
//     Copyright (c) Microsoft Corporation.  All rights reserved.
//     Licensed under the MIT License.  See License.txt in the project root for license information.
//     https://github.com/OData/odata.net/blob/7.5.0/src/Microsoft.OData.Client/Build.Portable/Microsoft.OData.Client.Portable.cs

namespace NewPlatform.Flexberry.ORM.ODataService.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// An accept header parser class.
    /// </summary>
    internal static class AcceptHeaderParser
    {
        /// <summary>Returns all MIME type directives from the specified <paramref name='text' />.</summary>
        /// <param name='text'>Text, as it appears on an HTTP Accepts header.</param>
        /// <returns>An enumerable object with MIME type directives.</returns>
        internal static IEnumerable<string> MimeTypeDirectivesFromAcceptHeader(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new string[] { };
            }

            return MimeTypesFromAcceptHeader(text)
                .Select(x => x.MimeType);
        }

        /// <summary>Returns all MIME types from the specified (non-blank) <paramref name='text' />.</summary>
        /// <param name='text'>Non-blank text, as it appears on an HTTP Accepts header.</param>
        /// <returns>An enumerable object with media type descriptions.</returns>
        private static IEnumerable<MediaType> MimeTypesFromAcceptHeader(string text)
        {
            Debug.Assert(!String.IsNullOrEmpty(text), "!String.IsNullOrEmpty(text)");
            List<MediaType> mediaTypes = new List<MediaType>();
            int textIndex = 0;
            while (!SkipWhitespace(text, ref textIndex))
            {
                ReadMediaTypeAndSubtype(text, ref textIndex, out string type, out string subType);

                while (!SkipWhitespace(text, ref textIndex))
                {
                    if (text[textIndex] == ',')
                    {
                        textIndex++;
                        break;
                    }

                    if (text[textIndex] != ';')
                    {
                        throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_MediaTypeRequiresSemicolonBeforeParameter);
                    }

                    textIndex++;
                    if (SkipWhitespace(text, ref textIndex))
                    {
                        // ';' should be a leading separator, but we choose to be a
                        // bit permissive and allow it as a final delimiter as well.
                        break;
                    }

                    SkipMediaTypeParameter(text, ref textIndex);
                }

                mediaTypes.Add(new MediaType(type, subType));
            }

            return mediaTypes;
        }

        /// <summary>Read a parameter for a media type/range.</summary>
        /// <param name="text">Text to read from.</param>
        /// <param name="textIndex">Pointer in text.</param>
        private static void SkipMediaTypeParameter(string text, ref int textIndex)
        {
            int startIndex = textIndex;
            if (ReadToken(text, ref textIndex))
            {
                throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_MediaTypeMissingValue);
            }

            string parameterName = text.Substring(startIndex, textIndex - startIndex);
            if (text[textIndex] != '=')
            {
                throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_MediaTypeMissingValue);
            }

            textIndex++;

            SkipQuotedParameterValue(parameterName, text, ref textIndex);
        }

        /// <summary>
        /// Reads Mime type parameter value for a particular parameter in the Content-Type/Accept headers.
        /// </summary>
        /// <param name="parameterName">Name of parameter.</param>
        /// <param name="headerText">Header text.</param>
        /// <param name="textIndex">Parsing index in <paramref name="headerText"/>.</param>
        /// <returns>String representing the value of the <paramref name="parameterName"/> parameter.</returns>
        private static void SkipQuotedParameterValue(string parameterName, string headerText, ref int textIndex)
        {
            // Check if the value is quoted.
            bool valueIsQuoted = false;
            if (textIndex < headerText.Length)
            {
                if (headerText[textIndex] == '\"')
                {
                    textIndex++;
                    valueIsQuoted = true;
                }
            }

            while (textIndex < headerText.Length)
            {
                char currentChar = headerText[textIndex];

                if (currentChar == '\\' || currentChar == '\"')
                {
                    if (!valueIsQuoted)
                    {
                        throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_EscapeCharWithoutQuotes(parameterName));
                    }

                    textIndex++;

                    // End of quoted parameter value.
                    if (currentChar == '\"')
                    {
                        valueIsQuoted = false;
                        break;
                    }

                    if (textIndex >= headerText.Length)
                    {
                        throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_EscapeCharAtEnd(parameterName));
                    }
                }
                else
                if (!IsHttpToken(currentChar))
                {
                    // If the given character is special, we stop processing.
                    break;
                }

                textIndex++;
            }

            if (valueIsQuoted)
            {
                throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_ClosingQuoteNotFound(parameterName));
            }
        }

        /// <summary>Reads the type and subtype specifications for a MIME type.</summary>
        /// <param name='text'>Text in which specification exists.</param>
        /// <param name='textIndex'>Pointer into text.</param>
        /// <param name='type'>Type of media found.</param>
        /// <param name='subType'>Subtype of media found.</param>
        private static void ReadMediaTypeAndSubtype(string text, ref int textIndex, out string type, out string subType)
        {
            Debug.Assert(text != null, "text != null");
            int textStart = textIndex;
            if (ReadToken(text, ref textIndex))
            {
                throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_MediaTypeUnspecified);
            }

            if (text[textIndex] != '/')
            {
                throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_MediaTypeRequiresSlash);
            }

            type = text.Substring(textStart, textIndex - textStart);
            textIndex++;

            int subTypeStart = textIndex;
            ReadToken(text, ref textIndex);

            if (textIndex == subTypeStart)
            {
                throw Error.HttpHeaderFailure(400, Strings.HttpProcessUtility_MediaTypeRequiresSubType);
            }

            subType = text.Substring(subTypeStart, textIndex - subTypeStart);
        }

        /// <summary>
        /// Reads a token on the specified text by advancing an index on it.
        /// </summary>
        /// <param name="text">Text to read token from.</param>
        /// <param name="textIndex">Index for the position being scanned on text.</param>
        /// <returns>true if the end of the text was reached; false otherwise.</returns>
        private static bool ReadToken(string text, ref int textIndex)
        {
            while (textIndex < text.Length && IsHttpToken(text[textIndex]))
            {
                textIndex++;
            }

            return (textIndex == text.Length);
        }

        /// <summary>
        /// Determines whether the specified character is a valid HTTP header token character.
        /// </summary>
        /// <param name="c">Character to verify.</param>
        /// <returns>true if c is a valid HTTP header token character; false otherwise.</returns>
        private static bool IsHttpToken(char c)
        {
            // A token character is any character (0-127) except control (0-31) or
            // separators. 127 is DEL, a control character.
            return c < '\x7F' && c > '\x1F' && !IsHttpSeparator(c);
        }

        /// <summary>
        /// Determines whether the specified character is a valid HTTP separator.
        /// </summary>
        /// <param name="c">Character to verify.</param>
        /// <returns>true if c is a separator; false otherwise.</returns>
        /// <remarks>
        /// See RFC 2616 2.2 for further information.
        /// </remarks>
        private static bool IsHttpSeparator(char c)
        {
            return
                c == '(' || c == ')' || c == '<' || c == '>' || c == '@' ||
                c == ',' || c == ';' || c == ':' || c == '\\' || c == '"' ||
                c == '/' || c == '[' || c == ']' || c == '?' || c == '=' ||
                c == '{' || c == '}' || c == ' ' || c == '\x9';
        }

        /// <summary>
        /// Skips whitespace in the specified text by advancing an index to
        /// the next non-whitespace character.
        /// </summary>
        /// <param name="text">Text to scan.</param>
        /// <param name="textIndex">Index to begin scanning from.</param>
        /// <returns>true if the end of the string was reached, false otherwise.</returns>
        private static bool SkipWhitespace(string text, ref int textIndex)
        {
            Debug.Assert(text != null, "text != null");
            Debug.Assert(text.Length >= 0, "text >= 0");
            Debug.Assert(textIndex <= text.Length, "text <= text.Length");

            while (textIndex < text.Length && Char.IsWhiteSpace(text, textIndex))
            {
                textIndex++;
            }

            return (textIndex == text.Length);
        }

        /// <summary>Use this class to represent a media type definition.</summary>
        [DebuggerDisplay("MediaType [{type}/{subType}]")]
        private sealed class MediaType
        {
            /// <summary>Sub-type specification (for example, 'plain').</summary>
            private readonly string subType;

            /// <summary>Type specification (for example, 'text').</summary>
            private readonly string type;

            /// <summary>
            /// Initializes a new <see cref="MediaType"/> read-only instance.
            /// </summary>
            /// <param name="type">Type specification (for example, 'text').</param>
            /// <param name="subType">Sub-type specification (for example, 'plain').</param>
            internal MediaType(string type, string subType)
            {
                Debug.Assert(type != null, "type != null");
                Debug.Assert(subType != null, "subType != null");

                this.type = type;
                this.subType = subType;
            }

            /// <summary>Returns the MIME type in standard type/subtype form, without parameters.</summary>
            internal string MimeType
            {
                get { return this.type + "/" + this.subType; }
            }
        }

        /// <summary>
        /// Strongly-typed and parameterized exception factory.
        /// </summary>
        private static class Error
        {
            /// <summary>
            /// Create and trace a HttpHeaderFailure
            /// </summary>
            /// <param name="errorCode">errorCode</param>
            /// <param name="message">message</param>
            /// <returns>InvalidOperationException</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", Justification = "errorCode ignored for code sharing")]
            internal static InvalidOperationException HttpHeaderFailure(int errorCode, string message)
            {
                return Trace(new InvalidOperationException(message));
            }

            /// <summary>
            /// Trace the exception
            /// </summary>
            /// <typeparam name="T">type of the exception</typeparam>
            /// <param name="exception">exception object to trace</param>
            /// <returns>the exception parameter</returns>
            private static T Trace<T>(T exception) where T : Exception
            {
                return exception;
            }
        }

        /// <summary>
        ///    Strongly-typed and parameterized string resources.
        /// </summary>
        private static class Strings
        {
            /// <summary>
            /// A string like "Media type is missing a parameter value."
            /// </summary>
            internal static string HttpProcessUtility_MediaTypeMissingValue
            {
                get
                {
                    return TextRes.GetString(TextRes.HttpProcessUtility_MediaTypeMissingValue);
                }
            }

            /// <summary>
            /// A string like "Media type requires a ';' character before a parameter definition."
            /// </summary>
            internal static string HttpProcessUtility_MediaTypeRequiresSemicolonBeforeParameter
            {
                get
                {
                    return TextRes.GetString(TextRes.HttpProcessUtility_MediaTypeRequiresSemicolonBeforeParameter);
                }
            }

            /// <summary>
            /// A string like "Media type requires a '/' character."
            /// </summary>
            internal static string HttpProcessUtility_MediaTypeRequiresSlash
            {
                get
                {
                    return TextRes.GetString(TextRes.HttpProcessUtility_MediaTypeRequiresSlash);
                }
            }

            /// <summary>
            /// A string like "Media type requires a subtype definition."
            /// </summary>
            internal static string HttpProcessUtility_MediaTypeRequiresSubType
            {
                get
                {
                    return TextRes.GetString(TextRes.HttpProcessUtility_MediaTypeRequiresSubType);
                }
            }

            /// <summary>
            /// A string like "Media type is unspecified."
            /// </summary>
            internal static string HttpProcessUtility_MediaTypeUnspecified
            {
                get
                {
                    return TextRes.GetString(TextRes.HttpProcessUtility_MediaTypeUnspecified);
                }
            }

            /// <summary>
            /// A string like "Value for MIME type parameter '{0}' is incorrect because it contained escape characters even though it was not quoted."
            /// </summary>
            internal static string HttpProcessUtility_EscapeCharWithoutQuotes(object o)
            {
                return TextRes.GetString(TextRes.HttpProcessUtility_EscapeCharWithoutQuotes, o);
            }

            /// <summary>
            /// A string like "Value for MIME type parameter '{0}' is incorrect because it terminated with escape character. Escape characters must always be followed by a character in a parameter value."
            /// </summary>
            internal static string HttpProcessUtility_EscapeCharAtEnd(object p0)
            {
                return TextRes.GetString(TextRes.HttpProcessUtility_EscapeCharAtEnd, p0);
            }

            /// <summary>
            /// A string like "Value for MIME type parameter '{0}' is incorrect because the closing quote character could not be found while the parameter value started with a quote character."
            /// </summary>
            internal static string HttpProcessUtility_ClosingQuoteNotFound(object p0)
            {
                return TextRes.GetString(TextRes.HttpProcessUtility_ClosingQuoteNotFound, p0);
            }
        }

        /// <summary>
        /// String resources utility class.
        /// </summary>
        private static class TextRes
        {
            internal const string HttpProcessUtility_MediaTypeMissingValue = "HttpProcessUtility_MediaTypeMissingValue";
            internal const string HttpProcessUtility_MediaTypeRequiresSemicolonBeforeParameter = "HttpProcessUtility_MediaTypeRequiresSemicolonBeforeParameter";
            internal const string HttpProcessUtility_MediaTypeRequiresSlash = "HttpProcessUtility_MediaTypeRequiresSlash";
            internal const string HttpProcessUtility_MediaTypeRequiresSubType = "HttpProcessUtility_MediaTypeRequiresSubType";
            internal const string HttpProcessUtility_MediaTypeUnspecified = "HttpProcessUtility_MediaTypeUnspecified";
            internal const string HttpProcessUtility_EscapeCharWithoutQuotes = "Value for MIME type parameter '{0}' is incorrect because it contained escape characters even though it was not quoted.";
            internal const string HttpProcessUtility_EscapeCharAtEnd = "Value for MIME type parameter '{0}' is incorrect because it terminated with escape character. Escape characters must always be followed by a character in a parameter value.";
            internal const string HttpProcessUtility_ClosingQuoteNotFound = "Value for MIME type parameter '{0}' is incorrect because the closing quote character could not be found while the parameter value started with a quote character.";

            internal static string GetString(string template, params object[] args)
            {
                if (args != null && args.Length > 0)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        String value = args[i] as String;
                        if (value != null && value.Length > 1024)
                        {
                            args[i] = value.Substring(0, 1024 - 3) + "...";
                        }
                    }
                    return String.Format(CultureInfo.CurrentCulture, template, args);
                }
                else
                {
                    return template;
                }
            }
        }
    }
}
#endif
