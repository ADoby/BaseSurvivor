using UnityEngine.Events;

public interface IHideable
{
    bool isVisible { get; }

    void SetVisibility(bool state, bool instant = false);

    void Show();

    void ShowInstant();

    void Hide();

    void HideInstant();

    void SetFinishedListener(UnityAction callback);

    float Duration(bool state);

    void SetPosition(object sender, float percentage, bool state);

    void SetDelay(bool state, float time);
}