using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace com.pigdawg.Hasher
{
    public class DataFormatter
    {
        [Flags]
        public enum DataColumns
        {
            Size =          0x01,
            HashValue =     0x02,
            FileName =      0x04,
            DirectoryName = 0x08,
            Attributes =    0x10,
            Created =       0x20,
            Modified =      0x40,
            Accessed =      0x80,
        }
        
        private class DataFields
        {
            public enum Field
            {
                Size,
                HashValue,
                FileName,
                DirectoryName,
                Attributes,
                Created,
                Modified,
                Accessed,
            }
            
            private readonly Dictionary<Field, string> m_fields = new Dictionary<Field, string>();
            
            public DataFields()
            {
                m_fields[Field.Size] = null;
                m_fields[Field.HashValue] = null;
                m_fields[Field.FileName] = null;
                m_fields[Field.DirectoryName] = null;
                m_fields[Field.Attributes] = null;
                m_fields[Field.Created] = null;
                m_fields[Field.Modified] = null;
                m_fields[Field.Accessed] = null;
            }
            
            public ICollection<Field> Fields
            {
                get
                {
                    return m_fields.Keys;
                }
            }
            
            public ICollection<string> Values
            {
                get
                {
                    return m_fields.Values;
                }
            }
            
            public string GetField(Field field)
            {
                if (!m_fields.ContainsKey(field))
                {
                    throw new ArgumentOutOfRangeException("field", "Unknown field name: \"" + field + "\"");
                }
                
                return m_fields[field];
            }
            
            public void SetField(Field field, string val)
            {
                if (!m_fields.ContainsKey(field))
                {
                    throw new ArgumentOutOfRangeException("field", "Unknown field: \"" + field + "\"");
                }
                
                switch (field)
                {
                    case Field.DirectoryName:
                    case Field.FileName:
                        m_fields[field] = string.Concat("\"", DataFormatter.EncodeControlCharacters(val), "\"");
                        break;
                    case Field.Attributes:
                        m_fields[field] = string.Concat("\"", val, "\"");
                        break;
                    default:
                        m_fields[field] = val;
                        break;
                }
            }
        }

        private readonly TextWriter m_writer;
        private ulong m_writes;

        public DataFormatter ()
        {
            m_writer = System.Console.Out;
        }

        public DataFormatter (TextWriter writer)
        {
            m_writer = writer;
        }

        public void WriteHeader ()
        {
            DataFields fields = new DataFields();
            
            fields.SetField(DataFields.Field.Size, "Size");
            fields.SetField(DataFields.Field.HashValue, "HashValue");
            fields.SetField(DataFields.Field.FileName, "FileName");
            fields.SetField(DataFields.Field.DirectoryName, "DirectoryName");
            fields.SetField(DataFields.Field.Attributes, "Attributes");
            fields.SetField(DataFields.Field.Created, "Created");
            fields.SetField(DataFields.Field.Modified, "Modified");
            fields.SetField(DataFields.Field.Accessed, "Accessed");
            
            this.WriteFields(fields);
        }

        public void WriteData (FileData data)
        {
            DataFields fields = new DataFields();
            
            fields.SetField(DataFields.Field.HashValue, Helper.HashValueToString(data.HashValue));
            fields.SetField(DataFields.Field.DirectoryName, data.DirectoryName.ToString());
            fields.SetField(DataFields.Field.FileName, data.Name.ToString());
            fields.SetField(DataFields.Field.Attributes, data.Attributes.ToString());
            fields.SetField(DataFields.Field.Size, data.Length.ToString());
            fields.SetField(DataFields.Field.Created, data.CreationTime.ToString());
            fields.SetField(DataFields.Field.Modified, data.LastWriteTime.ToString());
            fields.SetField(DataFields.Field.Accessed, data.LastAccessTime.ToString());
    
            this.WriteFields(fields);
        }
        
        private void WriteFields (DataFields fields)
        {
            StringBuilder sb = new StringBuilder ();
            
            foreach (string val in fields.Values)
            {
                sb.Append(val);
                sb.Append(",");
            }
            
            m_writer.WriteLine(sb.ToString());
            m_writes++;
            
            // Flush the output buffer after every tenth write
            if (0 == (m_writes % 10))
            {
                m_writer.Flush();
            }
        }
        
        private static string EncodeControlCharacters(string rawValue)
        {
            StringBuilder sb = new StringBuilder(rawValue);
            
            //****************************************************************
            //
            // Control character data supplied by Wikipedia at
            // http://en.wikipedia.org/wiki/Control_character
            //
            //   0 - 0x00 - (null, \0, ^@)
            //              originally intended to be an ignored character, but now
            //              used by many programming languages to terminate the end
            //              of a string.
            //   7 - 0x07 - (bell, \a, ^G)
            //              which may cause the device receiving it to emit a warning
            //              of some kind (usually audible).
            //   8 - 0x08 - (backspace, \b, ^H)
            //              used either to erase the last character printed or to
            //              overprint it.
            //   9 - 0x09 - (horizontal tab, \t, ^I)
            //              moves the printing position some spaces to the right.
            //  10 - 0x0A - (line feed, \n, ^J)
            //              used as the end_of_line marker in most UNIX systems and
            //              variants.
            //  12 - 0x0C - (form feed, \f, ^L)
            //              to cause a printer to eject paper to the top of the next
            //              page, or a video terminal to clear the screen.
            //  13 - 0x0D - (carriage return, \r, ^M)
            //              used as the end_of_line marker in Mac OS, OS-9, FLEX
            //              (and variants). A carriage return/line feed pair is used
            //              by CP/M-80 and its derivatives including DOS and Windows,
            //              and by Application Layer protocols such as HTTP.
            //  27 - 0x1B - (escape, \e [GCC only], ^[)
            // 127 - 0x7F - (delete, ^?)
            //              originally intended to be an ignored character, but now used to erase a character.
            //
            //****************************************************************
            
            //****************************************************************
            //
            // C# character escape sequences supplied by "C# Frequently Asked Questions" at
            // http://blogs.msdn.com/b/csharpfaq/archive/2004/03/12/what-character-escape-sequences-are-available.aspx
            //
            // \' - single quote, needed for character literals
            // \" - double quote, needed for string literals
            // \\ - backslash
            // \0 - Unicode character 0
            // \a - Alert (character 7)
            // \b - Backspace (character 8)
            // \f - Form feed (character 12)
            // \n - New line (character 10)
            // \r - Carriage return (character 13)
            // \t - Horizontal tab (character 9)
            // \v - Vertical quote (character 11)
            // \uxxxx - Unicode escape sequence for character with hex value xxxx
            // \xn[n][n][n] - Unicode escape sequence for character with hex value nnnn (variable length version of \uxxxx)
            // \Uxxxxxxxx - Unicode escape sequence for character with hex value xxxxxxxx (for generating surrogates)
            //
            //****************************************************************

            sb.Replace("\x00", "^@");

            sb.Replace("\x07", "^G");
            
            sb.Replace("\x08", "^H");

            sb.Replace("\x09", "^I");
            
            sb.Replace("\x0A", "^J");
            
            sb.Replace("\x0C", "^L");
            
            sb.Replace("\x0D", "^M");
            
            sb.Replace("\x1B", "^[");
            
            sb.Replace("\x7F", "^?");
            
            return sb.ToString();
            
         }
    }
}
