using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// author：孙宇
/// 2016/8/18
/// 可放置区域的判断
/// 有N个边界，其中任意一个边界不重合，一个矩形
/// 判断这个矩形是否可以放置
/// 1，先判断矩形的点是否在各个边界之外
/// 2，如果是，继续判断矩形的四个角是否在各个边界之外
/// 3，如果是，继续判断其它边界的两个对角是否在矩形外
/// 4，如果是，则此时的矩形是可以放置， 任何一步如果返回false 则此时矩形不可放置
/// </summary>
public class SmartPlace 
{
    private Vector4 bgboard;   //背景边界
    private List<RectBoard> boards;   //矩形边界
    private RectBoard curPoint;   //当前的矩形
    public RectBoard curRect
    {
        get
        {
            return curPoint;
        }
    }

    public SmartPlace()
    {
        boards = new List<RectBoard>();
    }
    public SmartPlace(Vector4 bg, List<RectBoard> boards, RectBoard curRect)
    {
        bgboard = bg;
        boards = new List<RectBoard>();
        this.boards = boards;
        this.curPoint = curRect;
    }

    public bool IsEmptyBgboard()
    {
        if (bgboard.x != 0 && bgboard.z != 0 && bgboard.y != 0 && bgboard.w != 0)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 判断矩形是否可以随意放置
    /// </summary>
    /// <returns></returns>
    public bool IsNewControl(Vector4 emptyPoint)
    {
        if (curPoint != null)
            return false;

        if (!IsOutofBoards(new Vector2((emptyPoint.x + emptyPoint.z) / 2.0f, (emptyPoint.y + emptyPoint.w) / 2.0f)))// 矩形内
        {
            //Debug.Log(((emptyPoint.x + emptyPoint.z) / 2.0f) + " " + ((emptyPoint.y + emptyPoint.w) / 2.0f));
            //Debug.Log(bgboard.x + " " + bgboard.z + " " + bgboard.y + " " + bgboard.w);
            //Debug.Log("IsOutofBoards is false");
            return false;
        }

        float cx = (emptyPoint.x + emptyPoint.z) / 2.0f + Math.Abs(emptyPoint.x - emptyPoint.z) / 2.0f;
        float cy = (emptyPoint.y + emptyPoint.w) / 2.0f + Math.Abs(emptyPoint.y - emptyPoint.w) / 2.0f;

        //Debug.Log(cx + " " + cy + "xiaojuan1");

        float mw = PublicFunction.GetWidth() * 1.0f * 400.0f / 1334.0f;
        float mh = PublicFunction.GetHeight() * 1.0f * 230.0f / 750.0f;

        //Debug.Log(mw + " " + mh);

        //控件不与UI区域重叠
        if (cx >= mw && cy >= mh)
        {
            //Debug.Log("IsOverlapUI is false");
            return false;
        }

        Vector2 point1 = new Vector2(emptyPoint.x, emptyPoint.y);
        Vector2 point2 = new Vector2(emptyPoint.x, emptyPoint.w);
        Vector2 point3 = new Vector2(emptyPoint.z, emptyPoint.w);
        Vector2 point4 = new Vector2(emptyPoint.z, emptyPoint.y);
        if (!IsOutofBoards(point1))
        {
            //Debug.Log("IsPoint1 is false");
            return false;
        }
        if (!IsOutofBoards(point2))
        {
            //Debug.Log("IsPoint2 is false");
            return false;
        }
        if (!IsOutofBoards(point3))
        {
            //Debug.Log("IsPoint3 is false");
            return false;
        }
        if (!IsOutofBoards(point4))
        {
            //Debug.Log("IsPoint4 is false");
            return false;
        }
        return IsOverlapBoards(emptyPoint);
    }
    /// <summary>
    /// 判断矩形是否可以放置
    /// </summary>
    /// <returns></returns>
    public bool IsOutBoards(Vector4 selectPos)
    {
        Vector2 point1 = new Vector2(selectPos.x, selectPos.y);
        Vector2 point2 = new Vector2(selectPos.x, selectPos.w);
        Vector2 point3 = new Vector2(selectPos.z, selectPos.w);
        Vector2 point4 = new Vector2(selectPos.z, selectPos.y);

        if (!IsOutofBoards(point1))
        {
            //Debug.Log("IsPoint1 is false");
            return false;
        }
        if (!IsOutofBoards(point2))
        {
            //Debug.Log("IsPoint2 is false");
            return false;
        }
        if (!IsOutofBoards(point3))
        {
            //Debug.Log("IsPoint3 is false");
            return false;
        }
        if (!IsOutofBoards(point4))
        {
            //Debug.Log("IsPoint4 is false");
            return false;
        }
        return true;
    }
    /// <summary>
    /// 判断矩形是否可以放置
    /// </summary>
    /// <returns></returns>
    public bool IsPlaceable()
    {
        if (curPoint == null)
        {
            //Debug.Log("curPoint is null");
            return false;
        }
        if(!IsOutofBoards(new Vector2((curPoint.board.x+curPoint.board.z)/2.0f,(curPoint.board.y+curPoint.board.w)/2.0f)))// 矩形内
        {
            //Debug.Log("IsOutofBoards is false");
            return false;
        }

        float cx = (curPoint.board.x + curPoint.board.z) / 2.0f + Math.Abs(curPoint.board.x - curPoint.board.z) / 2.0f;
        float cy = (curPoint.board.y + curPoint.board.w) / 2.0f + Math.Abs(curPoint.board.y - curPoint.board.w) / 2.0f;

        //Debug.Log(cx + " " + cy+"xiaojuan1");

        float mw = PublicFunction.GetWidth() * 1.0f * 400.0f / 1334.0f;
        float mh = PublicFunction.GetHeight() * 1.0f * 240.0f / 750.0f;

        //Debug.Log(mw + " " + mh);

        //控件不与UI区域重叠
        if (cx >= mw && cy >= mh)
        {
            //Debug.Log("IsOverlapUI is false");
            return false;
        }

        //Debug.Log((curPoint.board.x+curPoint.board.z)/2.0f+Math.Abs(curPoint.board.x - curPoint.board.z)/2.0f+" "+((curPoint.board.y+curPoint.board.w)/2.0f+Math.Abs(curPoint.board.y - curPoint.board.w)/2.0f)+"xiaojuan2");

        /*if (!IsOverlapBoards(curPoint.board))
        {
            Debug.Log("IsOverlapBoards is false");
            return false;
        }*/

        //判断顶点是否在边界内部
        Vector2 point1 = new Vector2(curPoint.board.x, curPoint.board.y);
        Vector2 point2 = new Vector2(curPoint.board.x, curPoint.board.w);
        Vector2 point3 = new Vector2(curPoint.board.z, curPoint.board.w);
        Vector2 point4 = new Vector2(curPoint.board.z, curPoint.board.y);
        if (!IsOutofBoards(point1))
        {
            //Debug.Log("IsPoint1 is false");
            return false;
        }
        if (!IsOutofBoards(point2))
        {
            //Debug.Log("IsPoint2 is false");
            return false;
        }
        if (!IsOutofBoards(point3))
        {
            //Debug.Log("IsPoint3 is false");
            return false;
        }
        if (!IsOutofBoards(point4))
        {
            //Debug.Log("IsPoint4 is false");
            return false;
        }
        return IsOverlapBoards(curPoint.board);
    }
    /// <summary>
    /// 检测当前的矩形中心是否在背景边界内
    /// </summary>
    /// <returns></returns>
    public bool IsOutofBoards(Vector2 curpos)
    {
        if(!IsInRect(bgboard, curpos))
            return false;
        /*foreach (var tem in boards)
        {
            if (IsInRect(tem.board, curpos))
                return false;
        }*/
        return true;
    }
    /// <summary>
    /// 检测当前的矩形是否与其他矩形边界有重叠
    /// </summary>
    /// <returns></returns>
    bool IsOverlapBoards(Vector4 curboard)
    {
       
        foreach (var tem in boards)
        {
            if (IsOverlapped(tem.board, curboard))
                return false;
        }
        return true;
    }
    /// <summary>
    /// 边界点是否在目标矩形之外
    /// </summary>
    /// <param name="boards"></param>
    /// <returns></returns>
    bool IsOutOfRect(List<RectBoard> boards)
    {
     //   Vector4 targetVect = new Vector4(curPoint.board.x - curPoint.board.z/2.0f,curPoint.board.y + curPoint.board.w/2.0f,curPoint.board.x + curPoint.board.z/2.0f,curPoint.board.y - curPoint.board.w/2.0f);
        foreach (var tem in boards)
        { 
            if(IsInRect(curPoint.board, new Vector2(tem.board.x,tem.board.y)))
                return false;
            if (IsInRect(curPoint.board, new Vector2(tem.board.y, tem.board.z)))
                return false;
        }
        return true;
    }
    /// <summary>
    /// 判断某个点是否在矩形内
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    bool IsInRect(Vector4 rect,Vector2 pos)
    {
        if (pos.x <= rect.x || pos.x >= rect.z || pos.y >= rect.y || pos.y <= rect.w)
            return false;
        return true;
    }
    /// <summary>
    /// 判断某个矩形是否和矩形重叠
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    bool IsOverlapped(Vector4 rect1, Vector4 rect2)
    {
        float mx = Math.Abs(rect1.x - rect1.z) / 2.0f + Math.Abs(rect2.x - rect2.z) / 2.0f;
        float my = Math.Abs(rect1.y - rect1.w) / 2.0f + Math.Abs(rect2.y - rect2.w) / 2.0f;
        float dx = Math.Abs((rect1.x + rect1.z) / 2.0f - (rect2.x + rect2.z) / 2.0f);
        float dy = Math.Abs((rect1.y + rect1.w) / 2.0f - (rect2.y + rect2.w) / 2.0f);

        if (dx >= mx || dy >= my)
        {
            //Debug.Log(rect1.x + " " + rect1.z + " " + rect1.y + " " + rect1.w);
            //Debug.Log(rect2.x + " " + rect2.z + " " + rect2.y + " " + rect2.w);
            //Debug.Log(mx + " " + my + " " + dx + " " + dy + " " + "false");
            //Debug.Log(false);
            return false;
        }
        else
        {
            //Debug.Log(mx + " " + my + " " + dx + " " + dy + " " + "true");
            return true;
        }
    }

    #region   // other 边界管理
    //删除和增加边界
    public void RemoveBoard(string name)
    {
        RectBoard rb = boards.Find((x) => x.name == name);
        if (rb != null)
            boards.Remove(rb);
    }
    public void RemoveBoard(RectBoard rb)
    {
        if (boards.Contains(rb))
            boards.Remove(rb);
    }
    public void AddBoard(RectBoard rb)
    {
        //Debuger.Log(rb.board.w + " " + rb.board.x + " " + rb.board.y + " " + rb.board.z);
        //Debuger.Log((rb.board.x + rb.board.z) / 2.0f + " " + (rb.board.y + rb.board.w) / 2.0f);
        boards.Add(rb);
    }

    /// <summary>
    /// 设置当前矩形 ， rect;x,y矩形的坐标点，w,z矩形的宽高
    /// </summary>
    /// <param name="rect"></param>
    public void SetCurRect(RectBoard rect)
    {
        curPoint = rect;
    }
    public void SetCurRect(string name)
    {
        //Debug.Log("rb is ready");
        RectBoard rb = boards.Find((x) => x.name == name);
        if (rb != null)
        {
            //Debug.Log("rb is this");
            boards.Remove(rb); //RemoveBoard(rb);
            SetCurRect(rb);
        }
    }
    public void ChangeCurPos(Vector4 rect)
    {
        //Debug.Log("curPoint is zero");
        if (curPoint != null)
        {
            curPoint.board = rect;
        }
    }
    public void SetBgBoard(Vector4 board)
    {
        bgboard = board;
    }
    public void SetBoards(List<RectBoard> boards)
    {
        if (boards != null)
            this.boards = boards;
    }
    public void Clear()
    {
        bgboard = Vector4.zero;
        boards.Clear();
    }
    #endregion

    public class RectBoard
    {
        public string name;
        public Vector4 board;
        public RectBoard(string nm, Vector4 re)
        {
            name = nm;
            board = re;
        }
    }
}
