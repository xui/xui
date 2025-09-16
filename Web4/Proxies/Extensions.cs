using Web4.WebApis;

namespace Web4.Proxies;

public static class ProxyExtensions
{
    extension(Event e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Animation e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.BeforeUnload e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Clipboard e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Composition e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.ContentVisibilityAutoStateChange e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.DeviceMotion e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.DeviceOrientation e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Drag e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Error e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Focus e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.FormData e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Gamepad e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.HashChange e) { public WindowProxy View => WindowProxy.Current; }
    extension<T>(Event.Input<T> e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Keyboard e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Message e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Mouse e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.PageTransition e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Pointer e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.PopState e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.PreventDefault e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Progress e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.PromiseRejection e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Storage e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Submit e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Toggle e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Touch e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Transition e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Wheel e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.Angles e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.Animation e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.Buttons e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.ClientXY e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.Coordinates e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.Data e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.DataTransfer e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.Deltas e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.Detail e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.DeviceMotion e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.DeviceOrientation e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.Error e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.HashChange e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.IsComposing e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.Keys e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.Length e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.ModifierAlt e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.ModifierCtrl e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.ModifierMeta e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.Modifiers e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.ModifierShift e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.MovementXY e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.OffsetXY e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.PageXY e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.Persisted e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.Pointer e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.Pressures e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.PreventDefault e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.Progress e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.RelatedTarget e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.ScreenXY e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.Skipped e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.States e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.Submitter e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.Target e) { public WindowProxy View => WindowProxy.Current; }
    extension<T>(Event.Subsets.Target<T> e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.Tilts e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.Touches e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.WidthHeight e) { public WindowProxy View => WindowProxy.Current; }
    extension(Event.Subsets.XY e) { public WindowProxy View => WindowProxy.Current; }
}
