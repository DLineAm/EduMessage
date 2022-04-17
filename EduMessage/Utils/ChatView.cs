using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace EduMessage.Utils
{
    public class ChatView : ListView
    {
        private uint _itemsSeen;
        private double _averageContainerHeight;
        private bool _processingScrollOffsets;
        private bool _processingScrollOffsetsDeffered;

        public ChatView()
        {
            IncrementalLoadingTrigger = IncrementalLoadingTrigger.None;

            ContainerContentChanging += UpdateRunningAverageContainerHeight;
        }

        protected override void OnApplyTemplate()
        {
            var scrollViewer = GetTemplateChild("ScrollViewer") as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ViewChanged += (s, e) =>
                {
                    StartProcessingDataVirtualizationScrollOffsets(ActualHeight);
                };
            }
            base.OnApplyTemplate();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var result = base.ArrangeOverride(finalSize);
            StartProcessingDataVirtualizationScrollOffsets(finalSize.Height);
            return result;
        }

        private async void StartProcessingDataVirtualizationScrollOffsets(double actualHeight)
        {
            if (_processingScrollOffsets)
            {
                _processingScrollOffsetsDeffered = true;
                return;
            }

            _processingScrollOffsets = true;

            do
            {
                _processingScrollOffsetsDeffered = false;
                await ProcessDataVirtualizationScrollOffsetsAsync(actualHeight);
            } while (_processingScrollOffsetsDeffered);

            _processingScrollOffsets = false;
        }

        private async Task ProcessDataVirtualizationScrollOffsetsAsync(double actualHeight)
        {
            var panel = ItemsPanelRoot as ItemsStackPanel;

            if (panel != null)
            {
                if ((panel.FirstVisibleIndex != -1 && panel.FirstVisibleIndex * _averageContainerHeight < actualHeight * IncrementalLoadingThreshold) ||
                    (Items.Count == 0))
                {
                    var virtualizingDataSource = ItemsSource as ISupportIncrementalLoading;

                    if (virtualizingDataSource != null)
                    {
                        if (virtualizingDataSource.HasMoreItems)
                        {
                            uint itemsToLoad = 1;

                            if (_averageContainerHeight != 0)
                            {
                                double avgItemsPerPage = actualHeight / _averageContainerHeight;

                                itemsToLoad = Math.Max((uint) (DataFetchSize * avgItemsPerPage), 1);
                            }

                            await virtualizingDataSource.LoadMoreItemsAsync(itemsToLoad);
                        }
                    }
                }
            }
        }

        private void UpdateRunningAverageContainerHeight(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.ItemContainer != null && !args.InRecycleQueue)
            {
                switch (args.Phase)
                {
                    case 0:

                        if (_averageContainerHeight == 0)
                        {
                            _averageContainerHeight = args.ItemContainer.DesiredSize.Height;
                        }

                        args.RegisterUpdateCallback(1, UpdateRunningAverageContainerHeight);
                        args.Handled = true;
                        break;

                    case 1:
                        args.ItemContainer.Content = args.Item;
                        args.RegisterUpdateCallback(2, UpdateRunningAverageContainerHeight);
                        args.Handled = true;
                        break;

                    case 2:
                        _averageContainerHeight =
                            (_averageContainerHeight * _itemsSeen + args.ItemContainer.DesiredSize.Height) /
                            ++_itemsSeen;
                        args.Handled = true;
                        break;
                }
            }
        }
    }
}