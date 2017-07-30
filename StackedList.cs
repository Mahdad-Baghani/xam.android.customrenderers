using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;
using Tag.Core;
using Xamarin.Forms;
using Tag.Core.Animations;
using System.Threading.Tasks;

namespace Tag.Core.Controls
{
    public class StackedList : Grid
    {
        private ICommand _innerSelectedCommand;
        private readonly StackLayout _itemsStackLayout;
        private readonly NoBarsScrollViewer _scrollView;
        public NoBarsScrollViewer ScrollView { get { return _scrollView; } }
        public StackLayout ItemStackLayout { get { return _itemsStackLayout; } }

        public event EventHandler SelectedItemChanged;

        public StackOrientation ListOrientation { get; set; }

        public double Spacing { get; set; }

        public static readonly BindableProperty SelectedCommandProperty =
            BindableProperty.Create("SelectedCommand", typeof(ICommand), typeof(StackedList), null);

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create("ItemsSource", typeof(IEnumerable), typeof(StackedList), default(IEnumerable), BindingMode.TwoWay, propertyChanged: ItemsSourceChanged);

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create("SelectedItem", typeof(object), typeof(StackedList), null, BindingMode.TwoWay, propertyChanged: OnSelectedItemChanged);

        public static readonly BindableProperty ItemTemplateProperty =
            BindableProperty.Create("ItemTemplate", typeof(DataTemplate), typeof(StackedList), default(DataTemplate));

        public ICommand SelectedCommand
        {
            get { return (ICommand)GetValue(SelectedCommandProperty); }
            set { SetValue(SelectedCommandProperty, value); }
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }
        
        private static void ItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var itemsLayout = (StackedList)bindable;
            itemsLayout.SetItems();
        }

        public StackedList()
        {
            Spacing = 5;
            _scrollView = new NoBarsScrollViewer();
            _itemsStackLayout = new StackLayout
            {
                Padding = Padding,
                Spacing = Spacing,
                //HorizontalOptions = LayoutOptions.FillAndExpand
                HorizontalOptions = LayoutOptions.EndAndExpand
            };
            _scrollView.Content = _itemsStackLayout;
            Children.Add(_scrollView);
        }

        protected virtual void SetItems()
        {
            _itemsStackLayout.Children.Clear();
            _itemsStackLayout.Spacing = Spacing;

            _innerSelectedCommand = new Command<View>(async view => {
                StoryBoard storyBoard = new StoryBoard();
                storyBoard.Animations.Add(new ScaleToAnimation { Scale = 1.025, Target = view, Duration = "25" });
                storyBoard.Animations.Add(new ScaleToAnimation { Scale = 1, Target = view, Duration = "25" });
                await storyBoard.Begin();

                SelectedItem = view.BindingContext;
                SelectedItem = null; // Allowing item second time selection
            });

            _itemsStackLayout.Orientation = ListOrientation;
            _scrollView.Orientation = ListOrientation == StackOrientation.Horizontal 
                ? ScrollOrientation.Horizontal 
                : ScrollOrientation.Vertical;

            if (ItemsSource == null)
            {
                return;
            }

            foreach (var item in ItemsSource)
            {
                _itemsStackLayout.Children.Add(GetItemView(item));
            }

            SelectedItem = null;
            Scroll_To_RTL_Start();
        }

        protected virtual View GetItemView(object item)
        {
            var content = ItemTemplate.CreateContent();
            var view = content as View;

            if (view == null)
            {
                return null;
            }

            view.BindingContext = item;

            var gesture = new TapGestureRecognizer
            {
                Command = _innerSelectedCommand,
                CommandParameter = view
            };

            AddGesture(view, gesture);

            return view;
        }

        private void AddGesture(View view, TapGestureRecognizer gesture)
        {
            view.GestureRecognizers.Add(gesture);

            var layout = view as Layout<View>;

            if (layout == null)
            {
                return;
            }

            foreach (var child in layout.Children)
            {
                AddGesture(child, gesture);
            }
        }

        private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var itemsView = (StackedList)bindable;
            if (newValue == oldValue && newValue != null)
            {
                return;
            }

            itemsView.SelectedItemChanged?.Invoke(itemsView, EventArgs.Empty);

            if (itemsView.SelectedCommand?.CanExecute(newValue) ?? false)
            {
                itemsView.SelectedCommand?.Execute(newValue);
            }
        }
        // neo : some added functionalities
        public void AddItem(object item)
        {
            if (ItemsSource != null)
            {
                List<object> ls = ItemsSource.Cast<object>().ToList();
                ls.Add(item);
                ItemsSource = ls as IEnumerable;
                SetItems();
            }
        }
        public async Task Scroll_To_RTL_Start()
        {
            await _scrollView.ScrollToAsync(_itemsStackLayout.Children[_itemsStackLayout.Children.Count() - 1] as Element, ScrollToPosition.End, true);
        }
    }
}
