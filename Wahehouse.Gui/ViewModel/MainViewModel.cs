using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using Warehouse.App;
using Warehouse.Domain.Entities;
using Warehouse.Domain.Parameters;
using Warehouse.Gui.Helpers;
using Warehouse.Gui.PreviewRenderer;

namespace Warehouse.Gui.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
		private Image _previewImage;
        private PreviewRendererViewModel PreviewRenderer => SimpleIoc.Default.GetInstance<PreviewRendererViewModel>();
        private readonly LayoutGenerator _layoutGenerator = new LayoutGenerator();
        private int _progressBarValue;
        private int[] _layoutCorridorGaps = new int[0];
        private int _layoutCorridorPallets;
        private int _layoutCorridorCount;
        private WarehouseLayout _warehouseLayout;
        private int _ordersCount = 5;

        public Image PreviewImage
		{
			get => _previewImage;
			set => Set(()=>PreviewImage, ref _previewImage, value);
		}

		public int ProgressBarValue
		{
			get => _progressBarValue;
			set => Set(() => ProgressBarValue, ref _progressBarValue, value);
		}

        public int LayoutCorridorCount
        {
            get => _layoutCorridorCount;
            set => Set(()=>LayoutCorridorCount, ref _layoutCorridorCount, value);
        }

        public int LayoutCorridorPallets
        {
            get => _layoutCorridorPallets;
            set => Set(() => LayoutCorridorPallets, ref _layoutCorridorPallets, value);
        }
	
        public int[] LayoutCorridorGaps
        {
            get => _layoutCorridorGaps;
            set => Set(() => LayoutCorridorGaps, ref _layoutCorridorGaps, value);
		}

        public WarehouseLayout WarehouseLayout
        {
            get => _warehouseLayout;
            set => Set(() => WarehouseLayout, ref _warehouseLayout, value);
        }

        public int OrdersCount
        {
            get => _ordersCount;
            set => Set(() => OrdersCount, ref _ordersCount, value);
        }

        public MainViewModel()
        {
            LayoutCorridorCount = 15;
            LayoutCorridorPallets = 80;
            LayoutCorridorGaps = new int[] { 20, 60 };
        }

		public ICommand GenerateLayout => new RelayCommand(() => { Task.Run(()=> ExecuteGenerateLayout()); },()=>LayoutCorridorPallets > 0 && LayoutCorridorCount>0);public ICommand FindPickingPaths => new RelayCommand(() => { Task.Run(()=> ExecuteFindPickingPaths()); },()=>LayoutCorridorPallets > 0 && LayoutCorridorCount>0 && OrdersCount > 0);

        public void ExecuteGenerateLayout()
        {
            var generator = _layoutGenerator;
            var layout = generator.GenerateLayout(LayoutCorridorCount, LayoutCorridorPallets, LayoutCorridorGaps)
                                  .FillWithArticles(80, 1000, 20)
                                  .GetLayout();

            var orders = generator.GetPickingOrders(5, 100, 100);
            WarehouseLayout = layout;

            layout.WarhouseOperationProgress += (sender, args) =>
                                                       {
                                                           ProgressBarValue = ((100 * 100 * args.Done) / args.Todo);
                                                       };
            PreviewRenderer.Clear();
            PreviewRenderer.LoadObstacles();
            PreviewRenderer.LoadPickingSlots();
            _warehouseLayout.GetTravelVertices();
            //_previewRenderer.LoadTravelVertices();

        }

        public void ExecuteFindPickingPaths()
        {
            //var pickingSolver = new PickingScanSolver(_warehouseLayout);
            //var pickingSolver = new PickingCoordSolver(_warehouseLayout);
            var pickingSolver = new PickingSolver(_warehouseLayout);
            
            var distanceSolverResults = new List<PathFindingResult<PickingTravelStep>>();
            var orders = _layoutGenerator.GetPickingOrders(OrdersCount, 100, 100);
            PreviewRenderer.ClearPickingPaths();
            foreach (var pickingOrder in orders)
            {
                var result = pickingSolver.FindPath(pickingOrder);
                if (result.Success)
                {
                    PreviewRenderer.AddPickingPathFindingResult(result);
                }

                distanceSolverResults.Add(result);
            }

            MessageBox.Show($"Avg: {distanceSolverResults.Average(x => x.Route.CostFromStart)} in {distanceSolverResults.Average(x=>x.ExecutionTime.TotalMilliseconds)} ms");

            //var pickingScanSolver = new PickingScanSolver(_warehouseLayout);
            //var scanSolverResult = new List<PathFindingResult<PickingTravelStep>>();
            //foreach (var pickingOrder in orders)
            //{
            //    var result = pickingScanSolver.FindPath(pickingOrder);

            //    scanSolverResult.Add(result);
            //}
        }
    }
}