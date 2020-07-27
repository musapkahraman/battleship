using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum ShipType
{
    Admiral = 0,
    VCruiser,
    HCruiser,
    VGunBoat,
    HGunBoat,
    Scout1,
    Scout2,
    Scout3,
    Scout4
}

public enum Marker
{
    Target = 9,
    Hit,
    Miss
}

public enum MapMode
{
    Disabled,
    Place,
    Attack
}

public class MapView : MonoBehaviour
{
    [SerializeField] private Camera cam;
    public GameSceneController controller;
    public Tilemap fleetLayer;
    public Tilemap markerLayer;
    public Tilemap cursorLayer;

    public Tile[] cursorTiles;
    public Tile cursorTile;

    public int size = 10;

    private Grid _grid;

    public Vector3Int minCoordinate;
    public Vector3Int maxCoordinate;

    private MapMode _mode;

    private void Start()
    {
        if (cam == null) cam = Camera.main;
        _grid = GetComponent<Grid>();
    }

    private void OnMouseDown()
    {
        var pos = cam.ScreenToWorldPoint(Input.mousePosition);
        var coordinate = _grid.WorldToCell(pos);

        coordinate.Clamp(minCoordinate, maxCoordinate);
        switch (_mode)
        {
            case MapMode.Place:
                controller.PlaceShip(coordinate);
                break;
            case MapMode.Attack:
                controller.TakeTurn(coordinate - new Vector3Int(size, 0, 0));
                break;
            case MapMode.Disabled:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Update()
    {
        if (_mode == MapMode.Disabled) return;

        var pos = cam.ScreenToWorldPoint(Input.mousePosition);
        var coordinate = _grid.WorldToCell(pos);

        coordinate.Clamp(minCoordinate, maxCoordinate);

        cursorLayer.ClearAllTiles();
        cursorLayer.SetTile(coordinate, cursorTile);
    }

    public void SetDisabled()
    {
        _mode = MapMode.Disabled;
    }

    public void SetPlacementMode()
    {
        _mode = MapMode.Place;
        minCoordinate = new Vector3Int(1, 0, 0);
        maxCoordinate = new Vector3Int(size - 1, size - 2, 0);
    }

    public void SetAttackMode()
    {
        _mode = MapMode.Attack;
        cursorTile = cursorTiles[(int) Marker.Target];
        minCoordinate = new Vector3Int(size + 1, 0, 0);
        maxCoordinate = new Vector3Int(size + size - 1, size - 2, 0);
    }

    public void SetShipCursor(ShipType shipType)
    {
        cursorTile = cursorTiles[(int) shipType];
    }

    public void SetShip(ShipType shipType, Vector3Int coordinate)
    {
        var index = (int) shipType;
        var tile = cursorTiles[index];
        fleetLayer.SetTile(coordinate, tile);
    }

    public void SetMarker(int index, Marker marker, bool radar)
    {
        var f = index / (size - 1);
        var coordinate = new Vector3Int(index % (size - 1) + 1, Mathf.FloorToInt(f), 0);
        // Debug.Log("Coordinate: " + coordinate + ":" + index + ":" + size + ": ");
        SetMarker(coordinate, marker, radar);
    }

    private void SetMarker(Vector3Int coordinate, Marker marker, bool radar)
    {
        if (radar)
        {
            coordinate += new Vector3Int(size, 0, 0);
        }

        markerLayer.SetTile(coordinate, cursorTiles[(int) marker]);
    }
}