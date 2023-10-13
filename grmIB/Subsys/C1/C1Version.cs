using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestAPI.C1
{
    class C1Version: IEquatable<C1Version>, IComparable, IComparable<C1Version>
    {
        static string[] Separators = { ".", "_" };

        int[] Version = new int[4];
        public bool SuccessConstruct { get; private set; }

        public C1Version() { }
        public C1Version(string strVer)
        {
            SetVersion(strVer);
        }
        public void SetVersion(string strVer)
        {
            bool rezult = false;
            string[] arrVer = strVer.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            if (arrVer.Length == 4)
            {
                rezult = true;
                for (int i = 0; i < 4; i++)
                {
                    bool intConvert = int.TryParse(arrVer[i], out Version[i]);
                    if (!intConvert) rezult = false;
                }
            }
            SuccessConstruct = rezult;
        }
        public override string ToString()
        {
            return String.Join<int>(".", Version);
        }

        ///
        /// IEquatable<C1Version>
        ///
        public override bool Equals(object other) // Переопределение Equals
        {
            if (!(other is C1Version)) return false;
            return Equals((C1Version)other);
        }
        public bool Equals(C1Version other) // Реализация IEquatable<Area>
        {
            bool rezult = true;
            for (int i = 0; i < 4; i++)
            {
                if (!(Version[i] == other.Version[i]))
                {
                    rezult = false;
                    break;
                }
            }
            return rezult;
        }
        public override int GetHashCode() // Переопределение GetHashCode
        {
            int rezult = Version[3];
            rezult += Version[2] * 1000;
            rezult += Version[1] * 100000;
            rezult += Version[0] * 10000000;
            return rezult;
        }
        public static bool operator ==(C1Version a1, C1Version a2) // Перегрузка оператора ==
        {
            return a1.Equals(a2);
        }
        public static bool operator !=(C1Version a1, C1Version a2) // Перегрузка оператора !=
        {
            return !a1.Equals(a2);
        }

        ///
        /// IComparable
        ///
        int IComparable.CompareTo(object other) // Необобщенный IComparable
        {
            if (!(other is C1Version))
                throw new InvalidOperationException("CompareTo: Not a C1Version");
            return CompareTo((C1Version)other);
        }

        ///
        /// IComparable<C1Version>
        ///
        public int CompareTo(C1Version other) // Обобщенный IComparable<T>
        {
            if (Equals(other)) return 0;
            int rezult = 0;
            for (int i = 0; i < 4; i++)
            {
                rezult = Version[i].CompareTo(other.Version[i]);
                if (rezult != 0) break;
            }
            return rezult;
        }
        public static bool operator <(C1Version n1, C1Version n2)
        {
            return n1.CompareTo(n2) < 0;
        }
        public static bool operator >(C1Version n1, C1Version n2)
        {
            return n1.CompareTo(n2) > 0;
        }
    }
}
