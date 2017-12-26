using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace Tab
{
    public sealed class TabControl : Control
    {
        public static readonly DependencyProperty ItemsTemplateProperty = DependencyProperty.Register(
            nameof(ItemsTemplate), typeof(DataTemplate), typeof(TabControl),
            new PropertyMetadata(default(DataTemplate)));

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource), typeof(IEnumerable), typeof(TabControl),
            new PropertyMetadata(default(IEnumerable), OnItemsSourceChanged));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem), typeof(object), typeof(TabControl),
            new PropertyMetadata(default(object), OnSelectedItemChanged));

        private readonly Dictionary<object, object> _items = new Dictionary<object, object>();

        private Button _addButton;
        private ContentControl _rootContainer;
        private ListView _tabList;

        public string TitlePath { get; set; }

        public TabControl()
        {
            DefaultStyleKey = typeof(TabControl);
        }

        public IEnumerable ItemsSource
        {
            get => (IEnumerable) GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public DataTemplate ItemsTemplate
        {
            get => (DataTemplate) GetValue(ItemsTemplateProperty);
            set => SetValue(ItemsTemplateProperty, value);
        }

        public event EventHandler AddRequest;

        private static void OnSelectedItemChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            (dependencyObject as TabControl)?.OnSelectedItemChanged(e.NewValue);
        }

        private void OnSelectedItemChanged(object newValue)
        {
            if (_rootContainer != null)
            {
                _rootContainer.Content = newValue != null ? _items[newValue] : null;
            }
        }

        private static void OnItemsSourceChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            (dependencyObject as TabControl)?.ItemsSourceChanged(e.NewValue as IEnumerable, e.OldValue as IEnumerable);
        }

        private void ItemsSourceChanged(IEnumerable newValue, IEnumerable oldValue)
        {
            _items.Clear();
            SelectedItem = null;
            if (oldValue is INotifyCollectionChanged oldCollection)
                oldCollection.CollectionChanged -= OnItemsSourceCollectionChanged;
            if (newValue is INotifyCollectionChanged newCollection)
                newCollection.CollectionChanged += OnItemsSourceCollectionChanged;
            if (newValue != null)
            {
                foreach (var item in newValue) AddContent(item);
                SelectedItem = _items.LastOrDefault().Key;
            }
        }

        private void AddContent(object item)
        {
            var content = ItemsTemplate.LoadContent();
            if (content is FrameworkElement frameworkElement) frameworkElement.DataContext = item;

            _items.Add(item, content);
        }

        private void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null && e.NewItems.Count > 0)
                    {
                        foreach (var item in e.NewItems) AddContent(item);
                        SelectedItem = _items.LastOrDefault().Key;
                    }

                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null && e.OldItems.Count > 0)
                    {
                        foreach (var item in e.OldItems) _items.Remove(item);
                        SelectedItem = _items.LastOrDefault().Key;
                    }

                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems != null && e.OldItems.Count > 0)
                        foreach (var item in e.OldItems)
                            _items.Remove(item);
                    if (e.NewItems != null && e.NewItems.Count > 0)
                    {
                        foreach (var item in e.NewItems) AddContent(item);

                        SelectedItem = e.NewItems[0];
                    }
                    else
                    {
                        SelectedItem = _items.LastOrDefault().Key;
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    _items.Clear();
                    if (sender is IEnumerable enumerable)
                    {
                        foreach (var item in enumerable) AddContent(item);
                        SelectedItem = _items.LastOrDefault().Key;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _tabList = GetTemplateChild("TabList") as ListView;
            _tabList.SetBinding(Selector.SelectedItemProperty, new Binding
            {
                Source = this,
                Path = new PropertyPath(nameof(SelectedItem)),
                Mode = BindingMode.TwoWay
            });
            _tabList.SetBinding(ItemsControl.ItemsSourceProperty, new Binding
            {
                Source = this,
                Path = new PropertyPath(nameof(ItemsSource)),
                Mode = BindingMode.OneWay
            });
            _tabList.ChoosingItemContainer += TabList_ChoosingItemContainer;
            _rootContainer = GetTemplateChild("RootContainer") as ContentControl;
            //_rootContainer.SetBinding(ContentControl.ContentProperty, new Binding
            //{
            //    Source = this,
            //    Path = new PropertyPath("SelectedItem.Value"),
            //    Mode = BindingMode.OneWay
            //});
            _addButton = GetTemplateChild("AddButton") as Button;
            _addButton.Click += AddButton_Click;
            SelectedItem = _items.LastOrDefault().Key;
            OnSelectedItemChanged(SelectedItem);
        }

        private void TabList_ChoosingItemContainer(ListViewBase sender, ChoosingItemContainerEventArgs args)
        {
            //if (args.ItemContainer != null)
            //{
            //    return;
            //}
            if (args.ItemContainer != null)
            {
                return;
            }
            var container = args.ItemContainer ?? new TabTitle();

            void ButtonClick(object s, RoutedEventArgs e)
            {
                (s as Button).Click -= ButtonClick;
                var shouldChangeSelectedItem = _items.LastOrDefault().Key == args.Item;
                (ItemsSource as IList)?.Remove(args.Item);
                if (shouldChangeSelectedItem)
                {
                    SelectedItem = _items.LastOrDefault().Key;
                }
            }

            void ContainerItemLoaded(object s, RoutedEventArgs e)
            {
                container.Loaded -= ContainerItemLoaded;
                var text = container.FindDescendant<TextBlock>();
                if (string.IsNullOrEmpty(TitlePath))
                {
                    text?.SetBinding(TextBlock.TextProperty, new Binding
                    {
                        Source = args.Item,
                        Mode = BindingMode.OneWay,
                    });
                }
                else
                {
                    text?.SetBinding(TextBlock.TextProperty, new Binding
                    {
                        Source = args.Item,
                        Path = new PropertyPath(TitlePath),
                        Mode = BindingMode.OneWay,
                    });
                }
                var button = container.FindDescendant<Button>();
                button.Click += ButtonClick;

            }
            container.Loaded += ContainerItemLoaded;
            args.ItemContainer = container;
        }


        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}