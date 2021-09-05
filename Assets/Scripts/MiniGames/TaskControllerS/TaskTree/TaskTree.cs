using System.Collections.Generic;
using UnityEngine;

public class TaskTree
    {
        public string name;
        public MapScript map;
        public List<TaskPoint> allPoints = new List<TaskPoint>(); //Все этапы готовки
        //TaskPoint[] startPoint; //точки, которые доступные сразу

        public TaskTree(PointData[] points, string newName)
        {
            name = newName;
            foreach (var point in points)
            {
                AddPoint(point.neeedPointsNums, point);
            }
        }

        public void CloseMap()
        {
            if (map != null)
                map.CloseMap();
        }

        public void SpawnMap()
        {
            map.SpawnStartObjects();
            map.ChangeMainText(name);
            foreach (var point in allPoints)
            {
                var state = point.enableNow ? CircleState.Enable : point.ended ? CircleState.Ended : CircleState.Disable;
                map.AddCircle(point, state);
            }
        }

        public void AddPoint(int[] needPointsNums, PointData gameInfo)
        {
            TaskPoint newPoint = new TaskPoint(gameInfo)
            {
                number = allPoints.Count
            }; //Создаем новую точку
            allPoints.Add(newPoint);
            int toLevel = 0;
            if (needPointsNums == null) { newPoint.enableNow = true; } //Если нет нужных точек, делаем сразу доступной
            else
            {
                newPoint.enableNow = false; //Если есть, то недоступной
                newPoint.fromPoints = new List<TaskPoint>(); //Создаем список нужных точек
                newPoint.needPointsNums = needPointsNums;
                foreach (var p in needPointsNums)
                {
                    toLevel = Mathf.Max(allPoints[p].level + 1, toLevel);
                    newPoint.fromPoints.Add(allPoints[p]); //Добавляем нужные точки из списка их номеров
                    allPoints[p].toPoint.Add(newPoint); //Добавляем нужным точкам ссылку на эту
                }
                newPoint.level = toLevel;
            }
        }

        public List<TaskPoint> EndPoint(TaskPoint taskPoint)
        {
            taskPoint.ended = true;
            taskPoint.enableNow = false;
            var result = new List<TaskPoint>();
            //Сделать allPoint[num] не активным, убрать из отображения
            foreach (var point in taskPoint.toPoint)
            {
                int i = 0;
                foreach(var ownPoint in point.fromPoints)
                {
                    if (ownPoint.ended == false) break;
                    i++;
                }

                if (i == point.fromPoints.Count)
                {
                    result.Add(point);
                    point.enableNow = true;
                    map.SetState(point.mapCircleObject, CircleState.Enable);
                } //Сделать это точку активной, добавить отображение
            }
            return result;
        }
    }