using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Warehouse.App;
using Warehouse.Domain.Entities;
using Warehouse.Domain.Interfaces;
using Warehouse.Domain.Parameters;
using Warehouse.Gui.PreviewRenderer.StateModels;
using Warehouse.Gui.ViewModel;

namespace Warehouse.Gui.PreviewRenderer
{
    public class PreviewRendererViewModel : ViewModelBase
    {
        private ObservableCollection<List<ILayoutElement>> _layoutElementCollections = new ObservableCollection<List<ILayoutElement>>();
        private MainViewModel _mainViewModel => SimpleIoc.Default.GetInstance<MainViewModel>();
        public WarehouseLayout WarehouseLayout => _mainViewModel.WarehouseLayout;
        private readonly object _layoutElementCollectionsLock = new object();

        public ObservableCollection<List<ILayoutElement>> LayoutElementCollections
        {
            get => _layoutElementCollections;
            set
            {
                Set(() => LayoutElementCollections, ref _layoutElementCollections, value);
                BindingOperations.EnableCollectionSynchronization(_layoutElementCollections, _layoutElementCollectionsLock);
            }
        }

        public void Clear()
        {
            LayoutElementCollections = new ObservableCollection<List<ILayoutElement>>();
        }

        public void LoadObstacles()
        {
            if (_mainViewModel.WarehouseLayout == null)
            {
                return;
            }

            if (LayoutElementCollections == null) LayoutElementCollections = new ObservableCollection<List<ILayoutElement>>();

            var obstacles = new List<ILayoutElement>(_mainViewModel.WarehouseLayout.GetObstacles());
            Application.Current.Dispatcher.InvokeAsync(() => { LayoutElementCollections.Add(obstacles); });
        }

        public void LoadTravelVertices()
        {
            if (_mainViewModel.WarehouseLayout == null)
            {
                return;
            }

            var vertices = new List<ILayoutElement>(_mainViewModel.WarehouseLayout.GetTravelVertices());
            Application.Current.Dispatcher.InvokeAsync(() => { LayoutElementCollections.Add(vertices); });
        }

        public void LoadPickingSlots()
        {
            if (_mainViewModel.WarehouseLayout == null)
            {
                return;
            }

            if (LayoutElementCollections == null) LayoutElementCollections = new ObservableCollection<List<ILayoutElement>>();

            var filledPickingSlots = new List<ILayoutElement>(_mainViewModel.WarehouseLayout.PickingSlots.Select(x => x.Value).Where(x => x.Units > 0).Select(x => new FilledPickingSlot(x)));
            var emptyPickingSlots = new List<ILayoutElement>(_mainViewModel.WarehouseLayout.PickingSlots.Select(x => x.Value).Where(x => x.Units == 0).Select(x => new EmptyPickingSlot(x)));
            Application.Current.Dispatcher.InvokeAsync(() => { LayoutElementCollections.Add(filledPickingSlots); });
            Application.Current.Dispatcher.InvokeAsync(() => { LayoutElementCollections.Add(emptyPickingSlots); });
            //LayoutElementCollections.Add(filledPickingSlots);
            //LayoutElementCollections.Add(emptyPickingSlots);
        }

        public void ClearPickingPaths()
        {
            foreach (var layoutElements in LayoutElementCollections.Where(x => (x.FirstOrDefault() is PickingTravelStep)))
            {
                Application.Current.Dispatcher.InvokeAsync(() => { LayoutElementCollections.Remove(layoutElements); });
            }
        }

        public void AddPickingPathFindingResult(IPathFindingResult pathFindingResult)
        {
            //var steps = pathFindingResult.PathCoordinates.Select(x => new CoordLayoutElement(x)).ToList();
            lock (LayoutElementCollections)
                Application.Current.Dispatcher.InvokeAsync(() => { LayoutElementCollections.Add(new List<ILayoutElement>() {pathFindingResult}); });
            {
                Application.Current.Dispatcher.InvokeAsync(() => { LayoutElementCollections.Add(new List<ILayoutElement>() {pathFindingResult}); });
            }
        }
    }
}