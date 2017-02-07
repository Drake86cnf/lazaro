using System;

namespace qGen
{
        /// <summary>
        /// Representa una expresi�n SQL literal.
        /// </summary>
        [Serializable]
        public class SqlExpression
        {
                public string Value = null;

                public SqlExpression(string expr)
                {
                        this.Value = expr;
                }

                public override string ToString()
                {
                        return this.Value;
                }
        }
}
