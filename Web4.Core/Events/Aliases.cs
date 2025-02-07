#pragma warning disable IDE1006 // Naming Styles

using Web4.Events.Subsets;

namespace Web4.Events;

public interface Aliases
{
    public interface Animation : IAnimationEvent { }
    public interface Base : IBaseEvent { }
    public interface Composition : ICompositionEvent { }
    public interface DeviceMotion : IDeviceMotionEvent { }
    public interface DeviceOrientation : IDeviceOrientationEvent { }
    public interface Drag : IDragEvent { }
    public interface Focus : IFocusEvent { }
    public interface HashChange: IHashChangeEvent { }
    public interface Input<T> : IInputEvent<T> { }
    public interface Keyboard : IKeyboardEvent { }
    public interface Mouse : IMouseEvent { }
    public interface Pointer: IPointerEvent { }
    public interface Progress : IProgressEvent { }
    public interface Submit : ISubmitEvent { }
    public interface Touch : ITouchEvent { }
    public interface Transition : ITransitionEvent { }
    public interface UI : IUIEvent { }
    public interface Wheel : IWheelEvent { }
    public interface Subsets
    {
        public interface Angles : IAnglesSubset { }
        public interface Animation : IAnimationSubset { }
        public interface Buttons : IButtonsSubset { }
        public interface ClientXY : IClientXYSubset { }
        public interface Coordinates : ICoordinatesSubset { }
        public interface Data : IDataSubset { }
        public interface DataTransfer : IDataTransferSubset { }
        public interface Deltas : IDeltasSubset { }
        public interface DeviceMotion : IDeviceMotionSubset { }
        public interface DeviceOrientation : IDeviceOrientationSubset { }
        public interface HashChange : IHashChangeSubset { }
        public interface IsComposing : IIsComposingSubset { }
        public interface Keys : IKeysSubset { }
        public interface ModifierAlt : IModifierAltSubset { }
        public interface ModifierCtrl : IModifierCtrlSubset { }
        public interface ModifierMeta : IModifierMetaSubset { }
        public interface Modifiers : IModifiersSubset { }
        public interface ModifierShift : IModifierShiftSubset { }
        public interface MovementXY : IMovementXYSubset { }
        public interface OffsetXY : IOffsetXYSubset { }
        public interface PageXY : IPageXYSubset { }
        public interface Pointer : IPointerSubset { }
        public interface Pressures : IPressuresSubset { }
        public interface Progress : IProgressSubset { }
        public interface RelatedTarget: IRelatedTargetSubset { }
        public interface ScreenXY : IScreenXYSubset { }
        public interface Submitter : ISubmitterSubset { }
        public interface Target : ITargetSubset { }
        public interface Target<T> : ITargetSubset<T> { }
        public interface Tilts : ITiltsSubset { }
        public interface Touches : ITouchesSubset { }
        public interface WidthHeight : IWidthHeightSubset { }
        public interface X : IXSubset { }
        public interface XY : IXYSubset { }
        public interface Y : IYSubset { }
    }
}

#pragma warning restore IDE1006 // Naming Styles
