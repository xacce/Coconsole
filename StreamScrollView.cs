using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace Coconsole
{
    public interface IPerfomantScrollViewDataSource<T>
    {
        IReadOnlyList<T> GetItemsRo();
    }

    public class StreamScrollView<T, TY> : VisualElement where TY : VisualElement
    {
        public delegate void OnRebindBatch(IReadOnlyList<T> items, Hierarchy hierarchy, int2 range);

        public delegate void OnRebind(T items, TY el);

        public override VisualElement contentContainer => _contentContainer;

        private readonly Func<TY> _onAdd;
        private readonly OnRebindBatch _onRebindBatch;
        private readonly int _itemHeight;
        private int _currentIndex;
        private int _onScreenCount;
        private bool _autoScrollToEnd = true;
        private readonly VisualElement _contentContainer;
        private readonly Scroller _scroller;
        private readonly OnRebind _onRebind;
        private readonly IPerfomantScrollViewDataSource<T> _items;
        private int _whellDeltaSum;
        private int _previousMax;
        private bool _sliderDraggedToEnd;
        private int _sliderNextValue;


        public StreamScrollView(
            IPerfomantScrollViewDataSource<T> items,
            int itemHeight,
            Func<TY> onAdd,
            OnRebind onRebind,
            OnRebindBatch onRebindBatch)
        {
            _items = items;
            AddToClassList("perfomant-scroll-view");
            _contentContainer = new VisualElement();
            _contentContainer.AddToClassList("perfomant-scroll-view__content-container");
            _scroller = new Scroller(0, 0, ValueChanged);
            _scroller.AddToClassList("perfomant-scroll-view__scroller");
            hierarchy.Add(_contentContainer);
            hierarchy.Add(_scroller);
            _onAdd = onAdd;
            _onRebindBatch = onRebindBatch;
            _onRebind = onRebind;
            _itemHeight = itemHeight;
            _onScreenCount = 1;
            _sliderNextValue = -1;
            RegisterCallback<WheelEvent>(We);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            Rebuild();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            Debug.Log($"Geometry changed: {evt.newRect.height}");
            var maxItems = Mathf.FloorToInt(evt.newRect.height / _itemHeight);

            if (_onScreenCount != maxItems)
            {
                _onScreenCount = maxItems;
                Rebuild();
            }
        }

        public void ThrottledUpdate()
        {
            // var scrollTo = Mathf.RoundToInt(_scroller.value);
            var items = _items.GetItemsRo();
            var itemsCount = items.Count;
            _scroller.highValue = items.Count;
            // Debug.Log($"{_previousMax} {itemsCount} {_currentIndex}, wheel delta: {_whellDeltaSum}, next: {_sliderNextValue}, dragged: {_sliderDraggedToEnd}");
            if (_previousMax > itemsCount)
            {
                Rebuild();
            }
            else if (_whellDeltaSum != 0)
            {
                RenderTo(_currentIndex + _whellDeltaSum, itemsCount);
                _whellDeltaSum = 0;
            }
            else if (_sliderNextValue != -1)
            {
                RenderTo(_sliderNextValue, itemsCount);
                _sliderNextValue = -1;
            }
            else if (_previousMax != itemsCount && (_currentIndex >= _previousMax - _onScreenCount || _sliderDraggedToEnd))
            {
                // Debug.Log($"prev: {_previousMax} curr: {items.Count}, index: {_currentIndex}");
                _scroller.slider.SetValueWithoutNotify(_scroller.highValue);
                RenderTo(itemsCount - 1, itemsCount);
            }

            _previousMax = itemsCount;
        }


        private void ValueChanged(float scroll)
        {
            _sliderDraggedToEnd = Mathf.Approximately(scroll, _scroller.highValue);
            _sliderNextValue = Mathf.RoundToInt(_scroller.value);
        }

        private void We(WheelEvent evt)
        {
            _whellDeltaSum += evt.delta.y > 0 ? 1 : -1;
        }


        private void Rebuild()
        {
            contentContainer.Clear();
            for (int i = 0; i < _onScreenCount; i++)
            {
                var tmp = _onAdd();
                Add(tmp);
            }

            var c = _items.GetItemsRo().Count;
            if (c != 0)
            {
                RenderTo(c - 1, c);
            }
        }

        private void RenderTo(int index, int itemsCount)
        {
            var items = _items.GetItemsRo();
            if (index < 0 || index > itemsCount) //bad index or items count so small
            {
                return;
            }


            var direction = math.clamp(index - _currentIndex, -1, 1);
            if (itemsCount < _onScreenCount) //partial rerender
            {
                Debug.LogWarning($"Partial rerender: index: {index}, items size: {itemsCount}, current: {_currentIndex}");
                _onRebindBatch(items, _contentContainer.hierarchy, new int2(0, itemsCount));
                _currentIndex = itemsCount;
            }
            else if (math.abs(_currentIndex - index) > _onScreenCount) //Full rerender
            {
                var start = index > _onScreenCount ? index - _onScreenCount : 0;
                var range = itemsCount > _onScreenCount ? _onScreenCount : itemsCount;
                Debug.LogWarning($"Full rerender: index: {index}, items size: {itemsCount}, offset: {start}, current: {_currentIndex}, range: {range}");
                _onRebindBatch(items, _contentContainer.hierarchy, new int2(start, range));
                _currentIndex = index;
            }
            else //rearrange rerender
            {
                var distance = math.abs(index - _currentIndex);
                Debug.LogWarning($"Rearrange rerender: index: {index}, items size: {itemsCount}, current: {_currentIndex}, distance: {distance}");
                for (int i = 0; i < distance; i++)
                {
                    var nextIndex = direction == 1 ? _currentIndex + 1 : _currentIndex - _onScreenCount;
                    if (nextIndex < 0 || nextIndex >= itemsCount)
                    {
                        return;
                    }

                    var el = contentContainer.ElementAt(direction == -1 ? contentContainer.childCount - 1 : 0) as TY;
                    contentContainer.Insert(direction == 1 ? contentContainer.childCount - 1 : 0, el);
                    _currentIndex += direction;
                    var item = items[nextIndex];
                    _onRebind(item, el);
                }
            }
        }
    }
}