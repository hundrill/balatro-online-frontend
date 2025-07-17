using System;

namespace BalatroOnline.Common
{
    /// <summary>
    /// 칩 표시 관련 유틸리티 클래스
    /// </summary>
    public static class ChipDisplayUtil
    {
        /// <summary>
        /// 숫자를 콤마가 포함된 문자열로 변환
        /// </summary>
        /// <param name="amount">변환할 숫자</param>
        /// <returns>콤마가 포함된 문자열 (예: "1,200")</returns>
        public static string FormatChipAmount(int amount)
        {
            return amount.ToString("N0");
        }

        /// <summary>
        /// 실버칩 표시용 문자열 반환
        /// </summary>
        /// <param name="amount">실버칩 수량</param>
        /// <returns>포맷된 실버칩 문자열</returns>
        public static string FormatSilverChip(int amount)
        {
            return FormatChipAmount(amount);
        }

        /// <summary>
        /// 골드칩 표시용 문자열 반환
        /// </summary>
        /// <param name="amount">골드칩 수량</param>
        /// <returns>포맷된 골드칩 문자열</returns>
        public static string FormatGoldChip(int amount)
        {
            return FormatChipAmount(amount);
        }
    }

    
}