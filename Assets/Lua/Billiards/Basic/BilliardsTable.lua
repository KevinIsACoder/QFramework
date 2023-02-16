--- 台球3D对象控制类
local BilliardsTable = {}
local self = BilliardsTable

local UE = UnityEngine
local BallStatus = BillardsType.BattleStatus
local EventDef = BillardsEvent.BillardsEventType
local BillardsModuleType = BillardsType.BillardsModuleType

-- 球的半径、直径和射线检测半径
BilliardsTable.BALL_RADIUS = 12
BilliardsTable.BALL_DIAMETER = BilliardsTable.BALL_RADIUS * 2
BilliardsTable.BALL_DIAMETER_SQUARE = BilliardsTable.BALL_DIAMETER * BilliardsTable.BALL_DIAMETER
BilliardsTable.BALL_SPHERE_RADIUS = BilliardsTable.BALL_RADIUS
BilliardsTable.POCKETBALL_SIZE = 20

function BilliardsTable.Init(onBeginDrag, onDrag, onEndDrag, onClick)
    if not self.table3D then
        self.InitTable()
    end
    self.Clear()
    self.table3D:SetActive(true)
    
    local touchEvent = UIEventListener.Get(self.eventListener.gameObject)
    touchEvent.onBeginDrag = onBeginDrag
    touchEvent.onDrag = onDrag
    touchEvent.onEndDrag = onEndDrag
    touchEvent.onClick = onClick

    local freeBallEvent = UIEventListener.Get(self.freeballListener.gameObject)
    freeBallEvent.onBeginDrag = onBeginDrag
    freeBallEvent.onDrag = onDrag
    freeBallEvent.onEndDrag = onEndDrag
    freeBallEvent.onClick = onClick

    self.eventListener:SetActive(false)
    self.freeBall:SetActive(false)
end

function BilliardsTable.InitTable()
    local preTable3D = Spark.Assets.LoadAsset("Game/Billiards/Table3D.prefab", typeof(GameObject))
    self.table3D = GameObject.Instantiate(preTable3D, Vector3.zero, Quaternion.identity)
    self.gameObject = self.table3D
    self.transform = self.table3D.transform

    self.cameraUI = GameObject.Find("UICamera"):GetComponent(typeof(Camera))
    local group = self.table3D:GetComponent(typeof(UIComponentGroup))
    
    self.camera3D = group:Get("camera3D")
    self.tableRoot = group:Get("tableRoot")
    self.ballParent = group:Get("ballGroup")
    self.downParent = group:Get("downPoint")
    self.ballInPos = group:Get("ballInPos").localPosition
    
    self.offset = 0
    self.balls = {}
    self.pockets = {}
    self.holeTrans = {}

    self.ball_line_len = 40

    -- 初始化洞口相关参数
    for i = 1, 6 do
        local hole = group:Get("hole_" .. i)
        local holeComp = hole:GetComponent(typeof(UIComponentGroup))
        local holePos = holeComp:Get("holePos").position

        local holeFenceTrans_1 = holeComp:Get("holeFenceTrans_1").position
        local holeFenceTrans_2 = holeComp:Get("holeFenceTrans_2").position
        local holeTrigger = holeComp:Get("holeTrigger")
        local bounds = holeTrigger.bounds
        local min, max = bounds.min, bounds.max
        
        local pocket = {
            index = i,
            bounds = {minX = math.fixed3(min.x), minY = math.fixed3(min.z), maxX = math.fixed3(max.x), maxY = math.fixed3(max.z)},
            hole = {x = math.fixed3(holePos.x), y = math.fixed3(holePos.z), r = math.fixed3(holePos.y)},
            fence1 = {x = math.fixed3(holeFenceTrans_1.x), y = math.fixed3(holeFenceTrans_1.z), r = math.fixed3(holeFenceTrans_1.y + BilliardsTable.BALL_RADIUS)},
            fence2 = {x = math.fixed3(holeFenceTrans_2.x), y = math.fixed3(holeFenceTrans_2.z), r = math.fixed3(holeFenceTrans_2.y + BilliardsTable.BALL_RADIUS)},
        }
        
        -- 预先计算与库边与球的距离平方
        pocket.hole.r2 = math.fixed3(pocket.hole.r * pocket.hole.r)
        pocket.fence1.r2 = math.fixed3(pocket.fence1.r * pocket.fence1.r)
        pocket.fence2.r2 = math.fixed3(pocket.fence2.r * pocket.fence2.r)

        self.pockets[i] = pocket
        self.holeTrans[i] = holeComp:Get("holePos")
        hole:SetActive(false)
    end

    -- 计算桌子大小
    local fenceBox = group:Get("fence")
    local bounds = fenceBox.bounds
    local min, max = bounds.min, bounds.max
    self.fence = {
        minX = math.fixed3(min.x + BilliardsTable.BALL_RADIUS),
        minY = math.fixed3(min.z + BilliardsTable.BALL_RADIUS),
        maxX = math.fixed3(max.x - BilliardsTable.BALL_RADIUS),
        maxY = math.fixed3(max.z - BilliardsTable.BALL_RADIUS)
    }
    fenceBox:SetActive(false)

    -- 初始化辅助线参数
    self.lines = group:Get("lines")
    self.ring = group:Get("ring")
    self.ring_error = group:Get("ring_error")
    self.ring_line = group:Get("ring_line")
    self.ball_line = group:Get("ball_line")
    self.cue_line = group:Get("cue_line")
    self.offset = 0

    self.freeBall = group:Get("freeball")
    self.freeForbid = group:Get("freeforbid")

    self.eventListener = group:Get("eventlistener")
    self.freeballListener = group:Get("freeballevent")
    
    -- 初始化球杆
    self.cueObject = group:Get("cueObject")

    self.downPos_1 = group:Get("downpos_1")
    self.downPos_2 = group:Get("downpos_2")
    self.downPos_3 = group:Get("downpos_3")
    -- 调整摄像机分辨率
    self.CalculateCameraSize()
end

function BilliardsTable.ConvertScreenToTable(pos)
    local worldPos = BillardsUtil.ConvertScreentoWorld(pos)
    local x, y = worldPos.x, worldPos.z
    y = y - self.offset
    return x, y
end

-- 处理其他人同步来的坐标
function BilliardsTable.UpdateFreeBallState(active, x, y, showForbid)
    self.freeBall:SetActive(active)
    self.freeForbid:SetActive(active and showForbid)
    if active then
        self.freeBall.transform:SetLocalPosition(x, 0, y)
    end
end

function BilliardsTable.SetPocketBalls(pocketBalls)
    SparkTween.KillAll()
    self.pocketAnimBalls = {}
    self.pocketBalls = pocketBalls
    local resetPos = self.downPos_3.transform.position

    for i = 1, #pocketBalls do
        local num = pocketBalls[i]
        local ball = self.GetOrCreateBall(num)
        if ball then
            local start_x = self.POCKETBALL_SIZE * (i - 1)
            local posX = resetPos.x + self.POCKETBALL_SIZE * 0.5 + start_x
            ball.component.enabled = false
            local trans = ball.transform
            trans:SetParent(self.downParent.transform, false)
            trans:SetPosition(posX, resetPos.y, resetPos.z)
            trans:SetLocalScale(self.POCKETBALL_SIZE, self.POCKETBALL_SIZE, self.POCKETBALL_SIZE)
        end
    end
end

function BilliardsTable.EnterPocket(num)
    table.insert(self.pocketAnimBalls, num)
    if #self.pocketAnimBalls == 1 then
        self.PlayEnterPocketAnim()
    end
end

function BilliardsTable.PlayEnterPocketAnim()
    if #self.pocketAnimBalls == 0 then return end

    local num = self.pocketAnimBalls[1]
    local ball = self.GetOrCreateBall(num)
    ball:SetCircleActive(false)
    local trans = ball.transform
    trans:SetParent(self.downParent.transform, false)
    local resetPos = self.downPos_1.position
    trans:SetPosition(resetPos.x, resetPos.y, resetPos.z) 
    trans:SetLocalScale(self.POCKETBALL_SIZE, self.POCKETBALL_SIZE, self.POCKETBALL_SIZE)
    local inPocketNum = #self.pocketBalls
    local pos_2 = self.downPos_2.transform.position
    local pos_3 = self.downPos_3.transform.position
    local start_x = inPocketNum * self.POCKETBALL_SIZE
    start_x = pos_3.x + self.POCKETBALL_SIZE * 0.5 + start_x

    table.insert(self.pocketBalls, num)
    SparkTween.Sequence()
        :Append(SparkTween.DOMove(trans, pos_2.x, pos_2.y, pos_2.z, 0.5)
            :OnComplete(function()
                table.remove(self.pocketAnimBalls, 1)
                self.PlayEnterPocketAnim()
            end))
        :Join(ball.ball:DOLocalRotate(-90, 0, 0, 0.5, Spark.Tweening.RotateMode.FastBeyond360))
        :Append(SparkTween.DOMove(trans, start_x, pos_3.y, pos_3.z, (pos_2.x - start_x) / (pos_2.x - pos_3.x) * 1.5))
        :Join(ball.ball:DOLocalRotate(0, 0, math.ceil(((pos_2.x - start_x) / (math.pi * self.POCKETBALL_SIZE)) * 360) , (pos_2.x - start_x) / (pos_2.x - pos_3.x) * 1.5, Spark.Tweening.RotateMode.FastBeyond360))
end

function BilliardsTable.GetOrCreateBall(num)
    local ball
    if self.balls then
        ball = self.balls[num]
        local parent = self.ballParent.transform
        if not ball then
            local ball3D = Spark.Assets.LoadAsset(string.format("Game/Billiards/Ball3dPrefab/ball_%d.prefab", num), typeof(GameObject))
            local go = GameObject.Instantiate(ball3D, parent)
            go.name = "ball_" .. num
            ball = go:GetLuaBehaviour("Game.Billiards.Basic.BilliardsBall")
            ball:SetNum(num)
            ball.transform:SetLocalScale(BilliardsTable.BALL_DIAMETER, BilliardsTable.BALL_DIAMETER, BilliardsTable.BALL_DIAMETER)
            self.balls[num] = ball
        else
            ball.transform:SetParent(parent, false)
            ball.gameObject:SetActive(true)
        end
        ball.transform:SetLocalScale(BilliardsTable.BALL_DIAMETER, BilliardsTable.BALL_DIAMETER, BilliardsTable.BALL_DIAMETER)
    end
    return ball
end

-- function BilliardsTable.AddBall(ball)
--     ball.component.enabled = true
--     ball.gameObject:SetActive(true)
-- end

-- function BilliardsTable.RemoveBall(ball)
--     ball.component.enabled = false
--     ball.gameObject:SetActive(false)
-- end

function BilliardsTable.ClearBalls()
    if self.balls then
        for _, ball in pairs(self.balls) do
            ball:SetVel(0, 0)
            ball:SetCircleActive(false)
            ball.gameObject:SetActive(false)
        end
    end
end

----场景适配
function BilliardsTable.CalculateCameraSize()
    local designSize = self.camera3D.orthographicSize
    local designAspect = 720 / 1280
    local widthOrthographicSize = designSize * designAspect
    local cameraAspect = self.camera3D.aspect
    if cameraAspect < designAspect then
        self.camera3D.orthographicSize = widthOrthographicSize / cameraAspect
    else
        self.camera3D.orthographicSize = designSize
    end
end

function BilliardsTable.CalculateTablePostion(uiPos)
    local worldPos = BillardsUtil.ConvertUIWorldtoSceneWorld(uiPos)
    self.tableRoot.transform:SetPosition(worldPos.x, 0, worldPos.z)
    self.offset = worldPos.z
    print("BilliardsTable Position+++++++++++++++", uiPos.x, uiPos.y, uiPos.z, worldPos.x, worldPos.z)
end

-- function Billards3D.RemoveBall(ballComp)
--     local ballNum = ballComp.ball_num
--     print("RemoveBall+++++++++=", ballComp.ball_num)
--     if ballNum ~= 0 and ballNum ~= 8 then
--         local ballObj = ballComp.gameObject
--         ballComp.component.enabled = false
--         ballObj.transform:SetParent(self.downParent.transform, false)
--         ballObj.transform.localPosition = Vector3.zero
--         ballObj.transform.localScale = Vector3(0.25, 0.25, 0.25)
--         local rigidBody = ballComp.rigidBody
--         rigidBody.isKinematic = false
--         ballObj:SetActive(true)
--     else
--         local ballObj = ballComp.gameObject
--         ballObj:SetActive(false)
--         ballComp.component.enabled = false
--         --BillardsInfo.addBall(ballComp.ball_num, ballComp)
--         -- local pos = Vector3(0, -100, 0)
--         -- ballComp:SetPosition(pos)      ----设置白球位置到桌台最外面，防止影响现有的球
--     end 
-- end

function BilliardsTable.Clear()
    self.pocketBalls = {}
    self.pocketAnimBalls = {}
    SparkTween.KillAll()
    self.ClearBalls()
    if self.table3D then
        self.table3D:SetActive(false)
    end
end

---是否禁用位置（摆放自由球判断）
function BilliardsTable.IsForbidPosition(pos)
    for _, v in pairs(self.balls) do
        local ballPos = v.transform.position
        local distance = Vector3.Distance(ballPos, pos)
        if distance <= self.BALL_DIAMETER then
            return true, v
        end
    end
    return false, nil
end

function BilliardsTable.Destroy()
    self.Clear()
    if self.table3D then
        GameObject.Destroy(self.table3D)
        self.table3D = nil
    end
end

function BilliardsTable.ConvertToWorldPos(x, y, z)
    y = y - self.offset
    return BillardsUtil.PackVec3(x, y, z)
end

function BilliardsTable.DrawLine(line, x1, y1, dir_x, dir_y, x2, y2, length)
    local r = BilliardsTable.BALL_RADIUS * 1.3
    local sx = x1 + r * dir_x
    local sy = y1 + r * dir_y
    
    local ex, ey
    if length then
        ex = sx + dir_x * length
        ey = sy + dir_y * length
    else
        ex = x2 + (-dir_x * r)
        ey = y2 + (-dir_y * r)
    end
    local dx, dy = (ex - sx), (ey - sy)

    line.transform:SetLocalPosition(sx + dx / 2, 0, sy + dy / 2)
    line.transform:SetLocalEulerAngles(90, 0, math.deg(math.atan(dir_y, dir_x)) - 90)
    line.size = Vector2(4, math.sqrt(dx * dx + dy * dy))
end

function BilliardsTable.DrawCueLines(show, isForbid, cue_x, cue_y, cue_dir_x, cue_dir_y, ring_x, ring_y, ring_dir_x, ring_dir_y, ball_num, ball_dir_x, ball_dir_y)
    if show then
        self.DrawLine(self.cue_line, cue_x, cue_y, cue_dir_x, cue_dir_y, ring_x, ring_y)
        if not isForbid and not (ring_dir_x == 0 and ring_dir_y == 0) then
            self.DrawLine(self.ring_line, ring_x, ring_y, ring_dir_x, ring_dir_y, nil, nil, self.ball_line_len)
        end
        self.ring_line:SetActive(not isForbid and not (ring_dir_x == 0 and ring_dir_y == 0))
        if ball_num and ball_dir_x and ball_dir_y then
            local bx, by = self.GetOrCreateBall(ball_num):GetPosition()
            if not isForbid then
                self.DrawLine(self.ball_line, bx, by, ball_dir_x, ball_dir_y, nil, nil, self.ball_line_len)
            end
            self.ball_line:SetActive(not isForbid)
            self.ring_error:SetActive(isForbid)
        else
            self.ball_line:SetActive(false)
            self.ring_error:SetActive(false)
        end
        self.ring:SetLocalPosition(ring_x, 0, ring_y)
    end
    self.lines:SetActive(show)
end

function BilliardsTable.DrawCue(show, isForbid, ball_x, ball_y, cue_dir_x, cue_dir_y, ring_x, ring_y, ring_dir_x, ring_dir_y, ball_num, ball_dir_x, ball_dir_y)
    if show then
        local r = BilliardsTable.BALL_RADIUS * 1.3
        local sx = ball_x - r * cue_dir_x
        local sy = ball_y - r * cue_dir_y

        self.cueObject:SetLocalPosition(sx, 0, sy)
        self.cueObject:SetLocalEulerAngles(90, 0, math.deg(math.atan(cue_dir_y, cue_dir_x)) - 90)
    end
    
    self.DrawCueLines(show, isForbid, ball_x, ball_y, cue_dir_x, cue_dir_y, ring_x, ring_y, ring_dir_x, ring_dir_y, ball_num, ball_dir_x, ball_dir_y)
    self.cueObject:SetActive(show)
    self.eventListener:SetActive(show)
end

function BilliardsTable.UpdateCuePower(ball_x, ball_y, dir_x, dir_y, powerValue)
    -- 从距离白球20的位置开始
    local r = BilliardsTable.BALL_RADIUS * 1.3
    local start_x = ball_x - r * dir_x
    local start_y = ball_y - r * dir_y
    -- 到距离白球100的位置结束P
    local radio = (powerValue / BillardsCfg.BilliardsValueCfg.MAX_F) * 100
    local current_x = start_x - dir_x * radio
    local current_y = start_y - dir_y * radio
    self.cueObject:SetLocalPosition(current_x, 0, current_y)
end

function BilliardsTable.GetBounds()
    return self.fence.minX, self.fence.minY, self.fence.maxX, self.fence.maxY
end

function BilliardsTable.SetBallLineConfig(length)
    self.ball_line_len = length
end

-- --- 改变球杆朝向和位置
-- function BilliardsTable.UpdateCueState(active, x, y, dirX, dirY, power)
--     self.DrawCue(active, x, y, dirX, dirY)

--     local px, py, vx, vy, snum, svx, svy = BilliardsUpdate:SimulateCast(dirX, dirY)
--     local ballComp = BilliardsBattle.GetBall(snum)
--     local cur_player = BilliardsBattle.GetCurrentPlayer()
--     local isForbid = cur_player:GetBallType() ~= 0 and cur_player:GetBallType() ~= ballComp:GetType()

--     self.DrawCueLines(true, isForbid, x, y, dirX, dirY, px, py, vx, vy, snum, svx, svy)
--     BilliardsBattle.SetHitPoint(px, py)
-- end

-- --- 改变球杆朝向和位置
-- function BilliardsTable.ChangeCueDirAndPos(dirX, dirY)
--     local x, y = BilliardsBattle.GetWhiteBall():GetPosition()
--     self.DrawCue(true, x, y, dirX, dirY)

--     local px, py, vx, vy, snum, svx, svy = BilliardsUpdate:SimulateCast(dirX, dirY)
--     local ballComp = BilliardsBattle.GetBall(snum)
--     local cur_player = BilliardsBattle.GetCurrentPlayer()
--     local isForbid = cur_player:GetBallType() ~= 0 and cur_player:GetBallType() ~= ballComp:GetType()

--     self.DrawCueLines(true, isForbid, x, y, dirX, dirY, px, py, vx, vy, snum, svx, svy)
--     BilliardsBattle.SetHitPoint(px, py)
-- end

return BilliardsTable
