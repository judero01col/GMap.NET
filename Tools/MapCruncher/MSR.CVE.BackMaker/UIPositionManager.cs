namespace MSR.CVE.BackMaker
{
    public class UIPositionManager : PositionUpdateIfc
    {
        private PositionUpdateIfc smUpdate;
        private PositionUpdateIfc veUpdate;
        private PositionMemoryIfc positionMemory;
        private bool slaved;
        private MapPosition _smPos;
        private MapPosition _vePos;
        private MapPosition slavedPos;

        public UIPositionManager(ViewerControl smViewer, ViewerControl veViewer)
        {
            _smPos = new MapPosition(this);
            _vePos = new MapPosition(this);
            smViewer.Initialize(GetSMPos, "Source Map Position");
            veViewer.Initialize(GetVEPos, "Virtual Earth Position");
            smUpdate = smViewer;
            veUpdate = veViewer;
            slaved = false;
        }

        public void SetPositionMemory(PositionMemoryIfc positionMemory)
        {
            this.positionMemory = positionMemory;
        }

        public MapPosition GetSMPos()
        {
            if (slaved)
            {
                return slavedPos;
            }

            return _smPos;
        }

        public MapPosition GetVEPos()
        {
            if (slaved)
            {
                return slavedPos;
            }

            return _vePos;
        }

        private void UpdatePositions()
        {
            smUpdate.PositionUpdated(GetSMPos().llz);
            veUpdate.PositionUpdated(GetVEPos().llz);
        }

        public void switchSlaved()
        {
            slaved = true;
            if (_vePos != null)
            {
                slavedPos = new MapPosition(_vePos, this);
            }
            else
            {
                slavedPos = new MapPosition(this);
            }

            _smPos = null;
            _vePos = null;
            UpdatePositions();
        }

        public void switchFree()
        {
            slaved = false;
            MapPosition prototype;
            if (slavedPos != null)
            {
                prototype = slavedPos;
            }
            else
            {
                prototype = new MapPosition(null);
            }

            _vePos = new MapPosition(prototype, this);
            _smPos = new MapPosition(prototype, this);
            slavedPos = null;
            UpdatePositions();
        }

        public void PositionUpdated(LatLonZoom llz)
        {
            PositionUpdated();
        }

        public void ForceInteractiveUpdate()
        {
            smUpdate.ForceInteractiveUpdate();
            veUpdate.ForceInteractiveUpdate();
        }

        internal void PositionUpdated()
        {
            UpdatePositions();
            if (positionMemory != null)
            {
                if (slaved)
                {
                    positionMemory.NotePositionLocked(GetVEPos());
                    return;
                }

                positionMemory.NotePositionUnlocked(GetSMPos().llz, GetVEPos());
            }
        }
    }
}
