namespace Marketplace.UI_OptimizationHandler;

public static class UI_Optimizations
{
    public enum OptimizationType
    {
        Horizontal,
        Vertical
    }

    public static IEnumerator UI_OptimizationCoroutine(int size, Func<bool> isVisible,
        IEnumerable<GameObject> Objects, OptimizationType type)
    {
        while (isVisible())
        {
            switch (type)
            {
                case OptimizationType.Horizontal:
                    int width = Screen.width;
                    foreach (GameObject go in Objects)
                    {
                        float pos = go.transform.position.x;
                        if (pos - size > width || pos - size < 0)
                        {
                            go.SetActive(false);
                        }
                        else
                        {
                            go.SetActive(true);
                        }
                    }

                    break;
                case OptimizationType.Vertical:
                    int height = Screen.height;
                    foreach (GameObject go in Objects)
                    {
                        float pos = go.transform.position.y;
                        if (pos - size > height || pos - size < 0)
                        {
                            go.SetActive(false);
                        }
                        else
                        {
                            go.SetActive(true);
                        }
                    }

                    break;
            }

            yield return null;
        }
    }
}

public static class ScrollView_Optimization
{
    private static readonly Dictionary<GameObject, Coroutine> Callers = new();

    public static void StartOptimization(GameObject caller, Func<bool> isVisible,
        IEnumerable<GameObject> Objects,
        UI_Optimizations.OptimizationType type)
    {
        if (Callers.TryGetValue(caller, out Coroutine coroutine))
            Marketplace._thistype.StopCoroutine(coroutine);
        if (!Objects.Any())
            return;

        int ObjectSize;
        if (type is UI_Optimizations.OptimizationType.Horizontal)
        {
            GameObject obj = Objects.First();
            ObjectSize = (int)(obj.GetComponent<RectTransform>().sizeDelta.x *
                               obj.transform.lossyScale.x) / 2;
        }
        else
        {
            GameObject obj = Objects.First();
            ObjectSize = (int)(obj.GetComponent<RectTransform>().sizeDelta.y *
                               obj.transform.lossyScale.y) / 2;
        }

        Callers[caller] = Marketplace._thistype.StartCoroutine(UI_Optimizations.UI_OptimizationCoroutine(ObjectSize, isVisible, Objects, type));
    }
}