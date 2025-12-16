namespace BidaTrader.Server.Helpers
{
    public static class SequentialUidHelper
    {
        public static string GenerateNextUid(string? lastUid)
        {
            // 1. Trường hợp chưa có user nào (Database rỗng)
            if (string.IsNullOrEmpty(lastUid))
            {
                return "AA0001";
            }

            // 2. Tách phần Chữ (AA) và phần Số (0001)
            string prefix = lastUid.Substring(0, 2); // 2 ký tự đầu
            string numberPart = lastUid.Substring(2); // 4 ký tự sau

            if (!int.TryParse(numberPart, out int number))
            {
                throw new Exception("UID cũ không đúng định dạng số.");
            }

            // 3. Tăng phần số
            number++;

            // 4. Kiểm tra logic tràn số (9999 -> 0001 và tăng chữ)
            if (number > 9999)
            {
                number = 1; // Reset về 0001
                prefix = IncrementPrefix(prefix); // Tăng phần chữ (AA -> AB)
            }

            // 5. Format lại kết quả (D4 nghĩa là số phải đủ 4 chữ số, vd: 0005)
            return $"{prefix}{number:D4}";
        }

        // Hàm tăng phần chữ cái (AA -> AB -> ... -> AZ -> BA -> ... -> ZZ)
        private static string IncrementPrefix(string prefix)
        {
            char[] chars = prefix.ToCharArray();

            // Tăng ký tự thứ 2
            chars[1]++;

            if (chars[1] > 'Z')
            {
                chars[1] = 'A'; // Reset ký tự 2 về A
                chars[0]++;     // Tăng ký tự 1

                if (chars[0] > 'Z')
                {
                    throw new Exception("Hệ thống đã hết kho số UID (Đạt giới hạn ZZ9999).");
                }
            }

            return new string(chars);
        }
    }
}