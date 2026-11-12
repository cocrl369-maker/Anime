using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AnimeFantasy
{
    /// <summary>Visual-Novel-artige Story-Szene: Portrait, Name, Text mit Schreibmaschinen-Effekt. Tippen = weiter.</summary>
    public class DialogueView : MonoBehaviour
    {
        StoryLine[] lines;
        Action onDone;
        int index = -1;
        bool typing;
        string fullText;

        Image leftPortrait, rightPortrait;
        Text nameText, bodyText, hintText;
        Image namePlate;
        Coroutine typeRoutine;

        public void Build(StoryLine[] storyLines, string chapterTitle, Action finished)
        {
            lines = storyLines;
            onDone = finished;
            var root = (RectTransform)transform;

            // Ganze Fläche ist ein Button: Tippen = weiter
            var hit = UIFactory.CreateImage("Tap", root, new Color(0, 0, 0, 0.25f));
            hit.raycastTarget = true;
            var tapButton = hit.gameObject.AddComponent<Button>();
            tapButton.transition = Selectable.Transition.None;
            tapButton.onClick.AddListener(Advance);

            UIFactory.CreateText("Chapter", root, chapterTitle, 46, UIFactory.Gold, TextAnchor.MiddleCenter, FontStyle.Bold)
                .rectTransform.Stretch(0.05f, 0.90f, 0.95f, 0.97f);

            leftPortrait = CreateBigPortrait(root, 0.02f, 0.62f);
            rightPortrait = CreateBigPortrait(root, 0.38f, 0.98f);

            var box = UIFactory.Panel("Box", root, new Color(0.06f, 0.04f, 0.14f, 0.94f));
            box.rectTransform.Stretch(0.03f, 0.06f, 0.97f, 0.36f);
            var border = UIFactory.Panel("Border", box.transform, new Color(1f, 1f, 1f, 0.06f));
            border.rectTransform.Stretch(0, 0, 1, 1);

            namePlate = UIFactory.Panel("NamePlate", root, new Color(0.85f, 0.35f, 0.45f));
            namePlate.rectTransform.Stretch(0.06f, 0.335f, 0.46f, 0.385f);
            nameText = UIFactory.CreateText("Name", namePlate.transform, "", 40, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);

            bodyText = UIFactory.CreateText("Body", box.transform, "", 44, Color.white, TextAnchor.UpperLeft);
            bodyText.resizeTextForBestFit = false;
            bodyText.rectTransform.Stretch(0.05f, 0.14f, 0.95f, 0.86f);

            hintText = UIFactory.CreateText("Hint", box.transform, "Tippen >>", 26, new Color(1, 1, 1, 0.5f), TextAnchor.LowerRight);
            hintText.rectTransform.Stretch(0.5f, 0.03f, 0.96f, 0.14f);

            var skip = UIFactory.CreateButton("Skip", root, "Überspringen", new Color(0.25f, 0.25f, 0.38f), Finish, 30);
            ((RectTransform)skip.transform).Stretch(0.66f, 0.38f, 0.97f, 0.43f);

            Advance();
        }

        static Image CreateBigPortrait(RectTransform root, float x0, float x1)
        {
            var holder = UIFactory.CreateRect("PortraitHolder", root).Stretch(x0, 0.40f, x1, 0.88f);
            var img = UIFactory.CreateImage("Portrait", holder, Color.white);
            img.preserveAspect = true;
            img.gameObject.SetActive(false);
            return img;
        }

        void Update()
        {
            if (hintText != null)
            {
                var c = hintText.color;
                c.a = typing ? 0f : 0.4f + 0.3f * Mathf.Sin(Time.unscaledTime * 5f);
                hintText.color = c;
            }
        }

        void Advance()
        {
            if (typing)
            {
                // Erst den aktuellen Text vollständig zeigen
                if (typeRoutine != null) StopCoroutine(typeRoutine);
                bodyText.text = fullText;
                typing = false;
                return;
            }

            index++;
            if (index >= lines.Length)
            {
                Finish();
                return;
            }
            ShowLine(lines[index]);
        }

        void ShowLine(StoryLine line)
        {
            var speaker = string.IsNullOrEmpty(line.Speaker) ? null : GameDatabase.GetCharacter(line.Speaker);
            bool isHero = speaker != null && Array.IndexOf(GameDatabase.HeroIds, speaker.Id) >= 0;

            if (speaker == null)
            {
                namePlate.gameObject.SetActive(false);
                bodyText.fontStyle = FontStyle.Italic;
            }
            else
            {
                namePlate.gameObject.SetActive(true);
                nameText.text = speaker.Name;
                namePlate.color = Color.Lerp(speaker.Color, Color.black, 0.35f);
                bodyText.fontStyle = FontStyle.Normal;

                // Helden links, Gegner rechts. Der Sprechende ist hell, der andere abgedunkelt.
                var target = isHero ? leftPortrait : rightPortrait;
                var other = isHero ? rightPortrait : leftPortrait;
                var sprite = UIFactory.LoadPortrait(speaker.Id);
                if (sprite != null)
                {
                    target.sprite = sprite;
                    target.color = Color.white;
                    target.gameObject.SetActive(true);
                    StartCoroutine(Pop(target.rectTransform));
                }
                if (other.gameObject.activeSelf) other.color = new Color(0.45f, 0.45f, 0.55f, 1f);
            }

            fullText = line.Text;
            typeRoutine = StartCoroutine(TypeText());
        }

        IEnumerator TypeText()
        {
            typing = true;
            bodyText.text = "";
            for (int i = 1; i <= fullText.Length; i++)
            {
                bodyText.text = fullText.Substring(0, i);
                yield return new WaitForSecondsRealtime(0.022f);
            }
            typing = false;
        }

        static IEnumerator Pop(RectTransform rt)
        {
            for (float t = 0; t < 0.18f; t += Time.unscaledDeltaTime)
            {
                rt.localScale = Vector3.one * Mathf.Lerp(0.94f, 1f, t / 0.18f);
                yield return null;
            }
            rt.localScale = Vector3.one;
        }

        void Finish()
        {
            if (onDone == null) return;
            var cb = onDone;
            onDone = null;
            cb();
        }
    }
}
