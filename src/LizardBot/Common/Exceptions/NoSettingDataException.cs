namespace LizardBot.Common.Exceptions
{
    /// <summary>
    /// Appsettings.json에서 설정값을 불러오지 못 했을 경우 발생하는 예외.
    /// </summary>
    /// <remarks>
    /// 기본 생성자.
    /// </remarks>
    /// <param name="key">appsettings.json의 key값.</param>
    public class NoSettingDataException(string key)
        : Exception($"appsettings.json에서 {key}값을 불러올 수 없습니다.")
    {
    }
}
