local BilliardsBattle = {}
local self = BilliardsBattle

BilliardsBattle.HIT_BALL_TIME = 30
BilliardsBattle.FREE_BALL_TIME = 15
BilliardsBattle.NET_REQUEST_INTERVAL = 300 --网络同步间隔300毫秒
local BillardsModuleType = BillardsType.BillardsModuleType
local BillardsEventType = BillardsEvent.BillardsEventType 
local BreakRuleType = BillardsType.BreakRuleType

function BilliardsBattle.Init()
end

-- 游戏模式
function BilliardsBattle.GetModeType()
    return VoiceInfo.GetConfigValue(BillardsType.FieldType)
end

function BilliardsBattle.SetModeType(modeType)
    if modeType == BillardsModuleType.EASY then
        BilliardsTable.SetBallLineConfig(80)
    else
        BilliardsTable.SetBallLineConfig(50)
    end
end

-- function BilliardsBattle.UpdateModeType()
--     local config_type_data = VoiceInfo.initGameData.game_status.config_type_data
--     if config_type_data then
--         config_type_data = json.decode(config_type_data)
--         for _, v in ipairs(config_type_data) do
--             self.modeType = v.value
--             break
--         end
--     end
-- end

function BilliardsBattle.InitCuePos()
    local white_ball = self.GetWhiteBall()
    local firstBall = self.GetFirstBall()
    
    if firstBall and white_ball then
        -- 白球坐标
        local fromPosX, fromPosY = white_ball:GetPosition()
        -- 第一个球的坐标
        local toPosX, toPosY = firstBall:GetPosition()

        local cueDirX, cueDirY = BillardsUtil.Vector2Normalize(toPosX - fromPosX, toPosY - fromPosY)
        self.SetCueDir(cueDirX, cueDirY, true)
    end
end

function BilliardsBattle.Reset()
    self.ballMap = {}
    self.ballList = {}
    self.playerMap = {}
    self.playerList = {}

    self.ResetRound()
end

function BilliardsBattle.ResetRound()
    -- 回合阶段
    self.roundPhase = BillardsType.RoundPhase.READY

    -- 初始化时间
    self.freeBallTime = BilliardsBattle.FREE_BALL_TIME
    self.countDowntTime = BilliardsBattle.HIT_BALL_TIME

    -- 当前操作的玩家
    self.currentPlayer = nil

    -- 球杆方向
    self.cueDir = {x = 0, y = 1}

    -- 第一个击球
    self.firstHitBall = nil

    -- 进袋的球
    self.ballsInPockets = {}
    
    --击球点（射线检测到的点)
    self.hitPoint = {x = 0, y = 0}

    ---上传数据间隔时间戳
    self.netRequestTime = 0

    self.tmpCueDirList = {}
    self.syncCueDirList = nil

    self.tmpFreeBallPos = {}  ---母球移动的中间过程
    self.syncFreeBallPos = nil

    self.lastCueData = nil
    self.lastFreeBallData = nil

    self.oldDragPos = {x = 0, y = 0}
end

function BilliardsBattle.ResetBlack8Pos(pos)
    local ball = self.GetBall(8) or self.AddBall(8, true)
    ball:SetVel(0, 0)
    ball:SetPosition(pos.x, pos.y)
    ball.gameObject:SetActive(true)
end

function BilliardsBattle.ResetWhiteBallPos(pos)
    local ball = self.GetBall(0) or self.AddBall(0, true)
    ball:SetVel(0, 0)
    ball:SetPosition(pos.x, pos.y)
    ball.gameObject:SetActive(true)
end

function BilliardsBattle.BeginRound(pos)
    self.ResetRound()
    --self.rule:BeginRound()
    -- 更换玩家
    self.currentPlayer = self.GetPlayerByPos(pos)
    
    --播放轮到玩家的音效
    if self.IsSelfCurPlayer(pos) then
        BilliardsAudio.PlaySound("Ball_notice_01")
    end
end

function BilliardsBattle.EndRound()
    -- 上报结果
    self.roundPhase = BillardsType.RoundPhase.READY
    if self.IsSelfCurPlayer() then
        self.rule:EndRound()
    end
end

function BilliardsBattle.OnBreakRule(data)
    -- 新回合
    self.BeginRound(data.next_player)

    if data.type == BreakRuleType.ReStart then
        local point = self.rule:GetWhiteBallPos(true)
        self.ResetWhiteBallPos(point)
        self.ChangeRoundPhase(BillardsType.RoundPhase.HIT_BALLS)
    elseif data.type == BreakRuleType.WhiteBallIn then  ---母球进袋
        self.ChangeRoundPhase(BillardsType.RoundPhase.FREE_BALL)
    elseif data.type == BreakRuleType.Black8In then
        self.ChangeRoundPhase(BillardsType.RoundPhase.FREE_BALL)
    elseif data.type == BreakRuleType.Delay then
        self.ChangeRoundPhase(BillardsType.RoundPhase.FREE_BALL)
    elseif data.type == BreakRuleType.NoBallHit then
        self.ChangeRoundPhase(BillardsType.RoundPhase.FREE_BALL)
    elseif data.type == BreakRuleType.NotOwnedBall then --击打的不是自己的球
        self.ChangeRoundPhase(BillardsType.RoundPhase.FREE_BALL)
    else
        self.ChangeRoundPhase(BillardsType.RoundPhase.HIT_BALLS)
    end
end

function BilliardsBattle.CheckRoundPhase(phase)
    return self.roundPhase == phase
end

function BilliardsBattle.ChangeRoundPhase(phase)
    if self.roundPhase ~= phase then
        self.roundPhase = phase
        local freeBall_X, freeBall_Y = 0, 0
        self.SyncCueDir()
        self.SyncFreeBallPos()
        if phase == BillardsType.RoundPhase.FREE_BALL then
            self.RemoveBall(0)
            local retPos = self.rule:GetWhiteBallPos(true)
            freeBall_X, freeBall_Y = retPos.x, retPos.y
        elseif phase == BillardsType.RoundPhase.HIT_BALLS then
            self.InitCuePos()
        end


        BilliardsTable.UpdateFreeBallState(phase == BillardsType.RoundPhase.FREE_BALL, freeBall_X, freeBall_Y)
        self.ShowBallEffect(phase == BillardsType.RoundPhase.HIT_BALLS)
        if (phase ~= BillardsType.RoundPhase.HIT_BALLS) then
            self.ShowCue(false)
        end
        EventManager.Dispatch(BillardsEventType.GAME_STATE_CHANGE, phase)
    end
end

function BilliardsBattle.SyncFreeBallPos()
    local data = self.lastFreeBallData
    if data and not self.IsSelfCurPlayer(data.pos) then
        self.lastFreeBallData = nil
        self.syncFreeBallPos = nil
        local finish = data.finish == 1
        BilliardsTable.UpdateFreeBallState(not finish, data.x, data.y)
        if finish then
            self.ResetWhiteBallPos(data)
            self.ChangeRoundPhase(BillardsType.RoundPhase.HIT_BALLS)
        end
    end
end

function BilliardsBattle.SyncFreeBallData(data)
    if not self.IsSelfCurPlayer(data.pos) then
        self.lastFreeBallData = data
        if  data.finish == 1 then
            self.SyncFreeBallPos()
        elseif data.ball_points then
            self.syncFreeBallPos = data.ball_points
        end
    end
end

function BilliardsBattle.GetFreeBallPoints()
    return self.syncFreeBallPos
end

function BilliardsBattle.IsSelfCurPlayer(pos)
    local player = pos and self.GetPlayerByPos(pos) or self.currentPlayer
    return player and player:GetUid() == UserInfo.GetUserId()
end

function BilliardsBattle.SetFreeBallPos(x, y, isFinish, isForbid)
    BilliardsTable.UpdateFreeBallState(not isFinish, x, y, isForbid)
    if isFinish then
        local pos = self.rule:GetValidPos({x = x, y = y})
        x, y = pos.x, pos.y
        self.ResetWhiteBallPos(pos)
        self.ChangeRoundPhase(BillardsType.RoundPhase.HIT_BALLS)
    end

    if self.IsSelfCurPlayer() then
        local is_finish = isFinish and 1 or 0
        local timestamp = TimeUtil.GetMilliseconds()
        if is_finish == 0 and timestamp - self.netRequestTime < self.NET_REQUEST_INTERVAL then
            table.insert(self.tmpFreeBallPos, {x = x, y = y})
            return
        end

        self.netRequestTime = timestamp
        BilliardsProxy.Net2S_5101(self.currentPlayer.pos, x, y, is_finish, self.tmpFreeBallPos)  ---母球摆放完毕
        self.tmpFreeBallPos = {}
    end
end

function BilliardsBattle.GetPlayers()
    return self.playerList or {}
end

function BilliardsBattle.GetCurrentPlayer()
    return self.currentPlayer
end

function BilliardsBattle.GetPlayerByPos(pos)
    if self.playerMap then
        return self.playerMap[pos]
    end
    return nil
end

function BilliardsBattle.GetBall(num)
    if self.ballMap then
        return self.ballMap[num]
    end
    return nil
end

function BilliardsBattle.GetWhiteBall()
    return self.GetBall(0)
end

function BilliardsBattle.GetLeftBalls()
    return self.ballList
end

function BilliardsBattle.GetPlayerCanHitBalls(pos)
    return self.GetPlayerByPos(pos):GetCanHitBalls()
end

function BilliardsBattle.SyncBalls(data)
    -- 桌上的球
    if data.points then
        ---剩余球位置
        for _, ballPoint in ipairs(data.points) do
            if ballPoint.num == 8 then
                self.ResetBlack8Pos(ballPoint)
            else
                local ballCom = self.GetBall(ballPoint.num) or self.AddBall(ballPoint.num, true)
                if ballCom then
                    ballCom:SetVel(0, 0)
                    ballCom:SetPosition(ballPoint.x, ballPoint.y)
                end
            end
        end
    end
    
    ---已进袋的球
    if data.pocket then
        if self.ballMap and self.ballList then
            for _, num in ipairs(data.pocket) do
                self.ballMap[num] = nil
                for i = #self.ballList, 1, -1 do
                    if self.ballList[i].num == num then
                        table.remove(self.ballList, i)
                        break
                    end
                end
            end
        end
        BilliardsTable.SetPocketBalls(data.pocket)
    end
end

--- 获取最前面的球
function BilliardsBattle.GetFirstBall()
    local leftBalls = self.GetLeftBalls()
    if leftBalls and #leftBalls > 0 then
        return leftBalls[1]
    end
    return nil
end

function BilliardsBattle._InitBalls(data)
    local initPoints = self.rule:GetInitPoints()
    for _, point in ipairs(initPoints) do
        local ball = self.AddBall(point.num, false)
        ball:SetPosition(point.x, point.y)
        ball.transform:SetLocalEulerAngles(90, 0, 0)
        ball.ball:SetLocalEulerAngles(0, 0, 0)
    end
    table.sort(self.ballList, function(a, b)
        if a.num == 0 then return false end
        if b.num == 0 then return true end
        return a.num < b.num
    end)
    
    -- 桌上的球
    if data.points then
        for _, point in ipairs(data.points) do
            local ball = self.ballMap[point.num]
            if point.num == 0 and data.in_open == 1 then
                -- 重置白球位置
                point = self.rule:GetWhiteBallPos(true)
            end
            ball:SetPosition(point.x, point.y)
            if data.in_open ~= 1 then
                ball:ChangeAngle((math.random() * 180) * math.pi / 180)
            end
        end
    end

    ---已进袋的球
    if data.pocket then
        for _, num in ipairs(data.pocket) do
            self.ballMap[num] = nil
            for i = #self.ballList, 1, -1 do
                if self.ballList[i].num == num then
                    table.remove(self.ballList, i)
                    break
                end
            end
        end
        BilliardsTable.SetPocketBalls(data.pocket)
    end
end


function BilliardsBattle.HandleFreeBallDrag(go, eventData, isEnd)
    if self.CheckRoundPhase(BillardsType.RoundPhase.FREE_BALL) then
        local x, y = BilliardsTable.ConvertScreenToTable(eventData.position)
        x = math.clamp(x, BilliardsTable.fence.minX, BilliardsTable.fence.maxX)
        y = math.clamp(y, BilliardsTable.fence.minY, BilliardsTable.fence.maxY)
        local forbid = self.IsFreeBallPosForbid(x, y)
        self.SetFreeBallPos(math.fixed3(x), math.fixed3(y), isEnd, forbid)
    end
end

function BilliardsBattle.IsFreeBallPosForbid(x, y)
    local balls = self.GetLeftBalls()
    for i = 1, #balls do
        local ball_x, ball_y = balls[i]:GetPosition()
        if math.abs(ball_x - x) < BilliardsTable.BALL_DIAMETER and math.abs(ball_y - y) < BilliardsTable.BALL_DIAMETER then
            return true
        end
    end
    return false
end

function BilliardsBattle.OnBeginDrag(go, eventData)
    if self.IsSelfCurPlayer() then
        self.isDrag = true
        self.HandleFreeBallDrag(go, eventData, false)
        self.oldDragPos = eventData.position
    end
end

function BilliardsBattle.OnEndDrag(go, eventData)
    if self.IsSelfCurPlayer() then
        self.isDrag = false
        self.HandleFreeBallDrag(go, eventData, true)
        self.HandleCueDrag(go, eventData, true)
    end
end

function BilliardsBattle.OnDrag(go, eventData)
    if not self.IsSelfCurPlayer() then return end
    self.isDrag = true
    self.HandleFreeBallDrag(go, eventData, false)
    self.HandleCueDrag(go, eventData, false)
end

function BilliardsBattle.HandleCueDrag(go, eventData, isEnd)
    if not self.IsSelfCurPlayer() then return end
    if self.CheckRoundPhase(BillardsType.RoundPhase.HIT_BALLS) then
        --白球坐标

        local ballX, ballY = self.GetWhiteBall():GetPosition()
        local startPosX, startPosY = BilliardsTable.ConvertScreenToTable(self.oldDragPos)
        local endPosX, endPosY = BilliardsTable.ConvertScreenToTable(eventData.position)

        local startDirX, startDirY = BillardsUtil.Vector2Normalize(startPosX - ballX, startPosY - ballY)
        local endDirX, endDirY = BillardsUtil.Vector2Normalize(endPosX - ballX, endPosY - ballY)
        
        local dotResult = BillardsUtil.Vector2Dot(startDirX, startDirY, endDirX, endDirY)
        dotResult = math.clamp(dotResult, -1.0, 1.0)
        local cueRadian = math.acos(dotResult) * 0.5
        local direct = startDirX * endDirY - endDirX * startDirY
        if direct < 0 then
           cueRadian = -cueRadian
        end

        local x, y = self.GetCueDir()
        local finalRadian = math.atan(y, x) + cueRadian
        x = math.cos(finalRadian)
        y = math.sin(finalRadian)
        self.SetCueDir(x, y, isEnd)
        self.oldDragPos = eventData.position
    end
end

function BilliardsBattle.OnClick(go)
    if not self.IsSelfCurPlayer() then return end
    if self.CheckRoundPhase(BillardsType.RoundPhase.HIT_BALLS) then
        if self.isDrag then return end
        local ballX, ballY = self.GetWhiteBall():GetPosition()
        local toPosX, toPosY = BilliardsTable.ConvertScreenToTable(Input.mousePosition)
        local dirX, dirY = BillardsUtil.Vector2Normalize(toPosX - ballX, toPosY - ballY)

        self.SetCueDir(dirX, dirY, true)
    end
end

function BilliardsBattle.NewMatch(data)
    -- 初始化所有数据
    self.Reset()

    -- 对局规则
    self.rule = Billiards8Rule()

    -- 初始化桌台
    BilliardsTable.Init(self.OnBeginDrag, self.OnDrag, self.OnEndDrag, self.OnClick)

    ---球桌上所有球的位置
    self._InitBalls(data)

    self.SetModeType(self.GetModeType())
    -- 初始化用户列表
    if data.users then
        for _, user in ipairs(data.users) do
            local player = BilliardsPlayer(user)
            self.playerMap[user.pos] = player
            table.insert(self.playerList, player)
        end
    end

    -- 指定当前玩家
    if data.cur_pos or data.pos then
        self.BeginRound(data.cur_pos or data.pos)
    end

    print("BilliardsBattle.NewMatch++++++++++", data.in_wait_hit, data.in_cue_move, data.in_hit, data.cur_pos, data.pos)
    
    -- 初始化当前轮次的阶段
    if data.in_wait_hit == 1 then
        -- 等待击球
        self.countDowntTime = data.wait_hit_timeout
        self.ChangeRoundPhase(BillardsType.RoundPhase.HIT_BALLS)
    elseif data.in_cue_move == 1 then
        -- 自由球，等待摆球
        self.freeBallTime = data.move_timeout
        self.ChangeRoundPhase(BillardsType.RoundPhase.FREE_BALL)
    elseif data.in_hit == 1 then
        -- 击球中
        local cueBall = self:GetWhiteBall()
        local posX, posY = cueBall:GetPosition()
        cueBall:SetPowerData(data.power, posX, posY, data.to_x, data.to_y)
        self.ChangeRoundPhase(BillardsType.RoundPhase.BALL_MOVING)
        -- 模拟计算球的位置和路径
        BilliardsUpdate:SimulateMove(data.hit_duration)
    else
        self.ChangeRoundPhase(BillardsType.RoundPhase.HIT_BALLS)
    end
    -- local EventDef = BillardsEvent.BillardsEventType
    -- EventManager.Dispatch(EventDef.E_BeginGame)
end

---通知击球
function BilliardsBattle.NoticeShotBall(data)
    if self.playerList then
        for _, player in ipairs(self.playerList) do
            player:UpdateConNum(data)
        end
    end

    self.BeginRound(data.pos)
    self.ChangeRoundPhase(BillardsType.RoundPhase.HIT_BALLS)
end

function BilliardsBattle.TrySetFirstBall(ball, target)
    if self.firstHitBall then return end
    if ball:GetNum() == 0 then
        self.firstHitBall = target:GetNum()
    end
end

function BilliardsBattle.AddBall(num, sort)
    local ball = BilliardsTable.GetOrCreateBall(num)
    if ball then
        ball:SetLive(true)
        ball:SetType(self.rule:GetBallType(num))
        ball.component.enabled = true
        self.ballMap[num] = ball
        self.ballList[#self.ballList + 1] = ball
        if sort then
            table.sort(self.ballList, function(a, b)
                if a.num == 0 then return false end
                if b.num == 0 then return true end
                return a.num < b.num
            end)
        end
    end
    return ball
end

function BilliardsBattle.RemoveBall(num)
    local ball = self.ballMap[num]
    if ball then
        ball:SetVel(0, 0)
        ball:SetLive(false)
        ball.gameObject:SetActive(false)
        ball.component.enabled = false
        self.ballMap[num] = nil
        for i = #self.ballList, 1, -1 do
            if self.ballList[i].num == num then
                table.remove(self.ballList, i)
                break
            end
        end
    end
end

function BilliardsBattle.EnterPocket(ball, pocketIndex)
    local num = ball:GetNum()
    local hitType = self.currentPlayer:GetBallType()
    local canHitBalls = self.currentPlayer:GetCanHitBalls()
    local validShot = self.rule:IsValidShot(num, hitType, canHitBalls, self.ballsInPockets)
    local shouldRespot = self.rule:ShouldRespot(num, hitType, canHitBalls, self.ballsInPockets)
    
    -- 移除落袋球
    self.RemoveBall(num)
    self.currentPlayer:RemoveBall(num)
    table.insert(self.ballsInPockets, num)

    -- 不需要重置的球播放落袋动画
    if not shouldRespot then
        BilliardsTable.EnterPocket(num)
    end

    -- 有效进球播放进球特效
    if validShot then
        EventManager.Dispatch(BillardsEvent.BillardsEventType.E_Enter_Ball, {holeIndex = pocketIndex})
    end
end

function BilliardsBattle.SyncCueDir()
    local data = self.lastCueData
    if data and not self.IsSelfCurPlayer(data.pos) then
        self.lastCueData = nil
        self.syncCueDirList = nil
        self.GetWhiteBall():SetPosition(data.x, data.y)
        --- 球杆朝向
        local dirX, dirY = BillardsUtil.Vector2Normalize(data.to_x - data.x, data.to_y - data.y)
        --- 被击的球
        local isForbid = false
        if data.ball_num ~= 0 then
            local ballComp = self.GetBall(data.ball_num)
            local cur_player = self.GetCurrentPlayer()

            if cur_player and ballComp then
                --- 被击的是否是自己的球
                isForbid = cur_player:GetBallType() ~= 0 and cur_player:GetBallType() ~= ballComp:GetType()
            end
        end
        --- data.to_x，data.to_y：圆环坐标
        --- data.vx, data.vy：白球反弹方向
        --- data.ball_num；被击球编号
        --- data.ball_vx, data.ball_vy：被击球反弹方向

        BilliardsTable.DrawCue(true, isForbid, data.x, data.y, dirX, dirY, data.to_x, data.to_y, data.vx, data.vy, data.ball_num ~= 0 and data.ball_num or nil, data.ball_vx, data.ball_vy)
    end
end

function BilliardsBattle.SyncCueData(data)
    if not self.IsSelfCurPlayer(data.pos) then
        if data.cue_dir and #data.cue_dir > 0 then
            self.syncCueDirList = data.cue_dir
        end
        self.lastCueData = data
    end
end

function BilliardsBattle.GetCueDirList()
    return self.syncCueDirList
end

function BilliardsBattle.SyncHitBall(data)
    if not self.IsSelfCurPlayer(data.pos) then
        local ballCom = self.GetWhiteBall()
        if ballCom then
            ballCom:SetPowerData(data.power, data.cue_pos.x, data.cue_pos.y, data.to_x, data.to_y)
            self.ChangeRoundPhase(BillardsType.RoundPhase.BALL_MOVING)
            BilliardsAudio.PlaySound("Ball_white_01")
        end
    end
end

function BilliardsBattle.ShowBallEffect(isShow)
    if not isShow then
        local leftBalls = self.GetLeftBalls()
        for i = 1, #leftBalls do
            leftBalls[i]:SetCircleActive(false)
        end
    else
        if self:IsSelfCurPlayer() then      ---只有当前击球者才会显示
            local curOper = self.GetCurrentPlayer()
            local leftBalls = self.GetPlayerCanHitBalls(curOper.pos)
            for i = 1, #leftBalls do
                local ball = self.GetBall(leftBalls[i])
                if ball then
                    ball:SetCircleActive(true)
                end
            end
        end
    end
end

function BilliardsBattle.HitBall(timeout, power)
    self.ShowCue(false)
    self.ShowBallEffect(false)
    if timeout then
        -- 超时
        BilliardsProxy.Net2S_5103(true)
    else
        BilliardsAudio.PlaySound("Ball_white_01")
        local ballCom = self.GetWhiteBall()
        local posX, posY = ballCom:GetPosition()
        local hit_x, hit_y = self.GetHitPoint()
        ballCom:SetPowerData(power, posX, posY, hit_x, hit_y)
        ----击球上传
        if self.IsSelfCurPlayer() then
            BilliardsProxy.Net2S_5103(false, self.currentPlayer.pos, posX, posY, hit_x, hit_y, power)
        end
        self.ChangeRoundPhase(BillardsType.RoundPhase.BALL_MOVING)
    end
end

function BilliardsBattle.ShowCue(isActive)
    BilliardsTable.DrawCue(isActive)
end

function BilliardsBattle.SetCueDir(x, y, sync)
    self.cueDir.x = x
    self.cueDir.y = y

    ----如果是当前玩家，传调整角度的数据
    if self.IsSelfCurPlayer() then
        local px, py, vx, vy, ball_num, ball_vx, ball_vy = BilliardsUpdate:SimulateCast(x, y)
        
        local isForbid = false
        if ball_num then
            local ballComp = self.GetBall(ball_num)
            local cur_player = self.GetCurrentPlayer()
            if ballComp and cur_player then
                isForbid = cur_player:GetBallType() ~= 0 and cur_player:GetBallType() ~= ballComp:GetType()
            end
        end
        
        local ball_x, ball_y = self.GetWhiteBall():GetPosition()
        BilliardsTable.DrawCue(true, isForbid, ball_x, ball_y, x, y, px, py, vx, vy, ball_num, ball_vx, ball_vy)
        self.SetHitPoint(px, py)

        -- 检查超时时间
        local timestamp = TimeUtil.GetMilliseconds()
        if (not sync) and timestamp - self.netRequestTime < self.NET_REQUEST_INTERVAL then
            table.insert(self.tmpCueDirList, {x = x, y = y})
            return
        end

        local cur_player = self.GetCurrentPlayer()
        BilliardsProxy.ChangeCueDirAndPosReq(cur_player.pos, ball_x, ball_y, px, py, vx, vy, ball_num or 0, ball_vx, ball_vy, self.tmpCueDirList)

        self.tmpCueDirList = {}
        self.netRequestTime = timestamp
    end
end

function BilliardsBattle.SetCuePower(power)
    local dirX, dirY = self.GetCueDir()
    local ball_x, ball_y = self.GetWhiteBall():GetPosition()
    BilliardsTable.UpdateCuePower(ball_x, ball_y, dirX, dirY, power)
end

function BilliardsBattle.GetCueDir()
    return self.cueDir.x, self.cueDir.y
end

function BilliardsBattle.SetHitPoint(x, y)
    self.hitPoint.x = x
    self.hitPoint.y = y
end

function BilliardsBattle.GetHitPoint()
    return self.hitPoint.x, self.hitPoint.y
end

function BilliardsBattle.GetInPocketBalls()
    return self.ballsInPockets
end

function BilliardsBattle.Clear()
    BilliardsTable.Clear()
end


return BilliardsBattle
