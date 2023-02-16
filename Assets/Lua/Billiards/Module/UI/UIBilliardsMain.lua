local UIBilliardsMain = class("UIBilliardsMain", BaseUIBilliardsMain)
local UIEventListener = CCM.UIEventListener
local BilliardsValueCfg = BillardsCfg.BilliardsValueCfg
local BillardsEventType = BillardsEvent.BillardsEventType

local math  = math
local atan	= math.atan
local cos = math.cos
local sin = math.sin
local sqrt = math.sqrt
local pi = math.pi

function UIBilliardsMain:OnOpened()
    self:InitData()
    self:InitUI()
    self:InitPlayers()
    self:RigisterEvent()
    self:UpdateBallBar()

    -- 调整桌台的位置
    if self._tablePos == nil then
        self._tablePos = self.view.tablePos.position
    end

    BilliardsTable.CalculateTablePostion(self._tablePos)

    if VoiceInfo.initGameData.game_status.room_status == 0 then
        BilliardsAudio.PlaySound("Ball_opening_01")
        local vsContent = {[1] = {showType = 1}}
        self.root:OpenAsync(UIBilliardsPrompt, UIContext.Get({showContent = vsContent, callback = function()
            self:UpdateGameState()
            self:NoticeShotBall()
        end}))
    else
        self:UpdateGameState()
    end

end

function UIBilliardsMain:InitData()
    self.m_powerValue = 0
    self.leftTime = 0
end

function UIBilliardsMain:InitUI()
    local sliderEvent = UIEventListener.Get(self.view.sliderPower.gameObject)
    sliderEvent.onUp = handler(self.OnSliderPointUp, self)

    local angleCtrlEvent = UIEventListener.Get(self.view.angleSlider.gameObject)
    angleCtrlEvent.onDrag = handler(self.OnDrag, self)
    angleCtrlEvent.onEndDrag = handler(self.OnEndDrag, self)

    for i = 1, 6 do
        self.view["spineEnterBall_"..i]:SetActive(false)
    end
    self.view.sliderPower.enabled = false
    self.view.angleSlider.enabled = false
end

function UIBilliardsMain:InitPlayers()
    local players = BilliardsBattle.GetPlayers()
    for _, player in pairs(players) do
        local index = player.pos + 1
        local objHead = self.view["playerHead_"..index].transform:GetChild(0)
        UIHelper.SetHead(objHead, player.headUrl, self:GetLoader(), function()
            VoiceSender.OperateVoiceChatSeat(VoiceInfo.GetVoiceSeatId(player:GetUid()))
        end)
    end
end

---注册事件监听
function UIBilliardsMain:RigisterEvent()
    self.eventManager:Add(BillardsEventType.GAME_STATE_CHANGE, function(_, data) self:UpdateGameState() end)

    self.eventManager:Add(BillardsEventType.E_UpdateBall, function(_, data) self:UpdateBallBar() end)
    self.eventManager:Add(BillardsEventType.E_Enter_Ball, function(_, data) self:OnEnterBall(data) end)
    self.eventManager:Add(BillardsEventType.E_BreakRule, function(_, data) self:OnBreakRule(data) end)
    self.eventManager:Add(BillardsEventType.E_SeprateBall, function(_, data) self:NoticeSeprateBall(data) end)
    self.eventManager:Add(BillardsEventType.E_NoticeShotBall, function(_, data) self:NoticeShotBall(data) end)

    self.eventManager:Add(BillardsEventType.E_GameOver, function(_, data)
        self:StopTimer()                                ---游戏结束后结束计时器
        if self.root:Exists(UIBilliardsPrompt) then     ---有提示的时候先弹提示，再弹结算
            self.scheduler:Timeout(function()
                self.root:Open(UIBilliardsResult, UIContext.Get({resultdata = data.users}))
            end, 5)
        else
            self.root:Open(UIBilliardsResult, UIContext.Get({resultdata = data.users}))
        end
    end)
end

function UIBilliardsMain:UpdateBallBar()
    local players = BilliardsBattle.GetPlayers()
    if players then
        for _, player in pairs(players) do
            local balls = player:GetCanHitBalls()
            local index = player.pos + 1
            local trans = self.view["playerBallBar_"..index].transform
            local obj = self.view["playerBall_"..index].gameObject
            UIHelper.CreatePart(obj, trans, #balls, function(index, part)
                local ballImg = part:Get("ballImg")
                local id = balls[index]
                UIHelper.SetSprite(ballImg, "ball"..id)
            end)
        end
    end
end

function UIBilliardsMain:ShowTimer(deltaTime, total_time, pos, callBack)
    self:StopTimer()
    local img = self.view["playerTimeImg_"..pos + 1]
    img.gameObject:SetActive(true)
    local endTime = TimeUtil.GetMilliseconds() / 1000 + deltaTime
    self.coutDownTimer = self.scheduler:Interval(function()
        local curTime = TimeUtil.GetMilliseconds() / 1000
        self.leftTime = endTime - curTime
        if self.leftTime <= 0 then
            self:StopTimer()
            if callBack then callBack() end
            return 
        end
        if self.leftTime > 3 and self.leftTime <= 5 then
            if not self.isPlayingSound then 
                self.isPlayingSound = true
                BilliardsAudio.PlaySound("Ball_warning_01")
            end 
        end
        local delta = self.leftTime / total_time
        img.fillAmount = delta
    end, 0.05)
end

function UIBilliardsMain:OnBreakRule(data)
    local BreakRuleType = BillardsType.BreakRuleType
    local strKey = ""
    if data.type == BreakRuleType.ReStart then
        strKey = "Billiards_BreakRule"
    elseif data.type == BreakRuleType.WhiteBallIn then  ---母球进袋
        strKey = "Billiards_BreakRule"
    elseif data.type == BreakRuleType.Black8In then
        strKey = "Billiards_BreakRule"
    elseif data.type == BreakRuleType.Delay then
        BilliardsAudio.PlaySound("Ball_overtime_01")
        strKey = "Billiards_BreakRule"
    elseif data.type == BreakRuleType.NoBallHit then
        strKey = "Billiards_BreakRule"
    elseif data.type == BreakRuleType.NotOwnedBall then
        strKey = "Billiards_BreakRule"
    elseif data.type == BreakRuleType.RestartDelay then
        strKey = BilliardsBattle.IsSelfCurPlayer(data.pos) and "Billiards_Delay" or "Billiards_OthersDelay"
    end

    local playerTip = ""
    if BilliardsBattle.IsSelfCurPlayer(data.pos) then ----只有犯规玩家弹的提示,提示玩家第几次超时犯规
        if data.type == BreakRuleType.Delay or data.type == BreakRuleType.RestartDelay then
            local s = string.format(I18nManager.GetCurrentValue("Billiards_DelayTimes"), data.foul_num)
            playerTip = I18nManager.ApplyRTLfix(s)
        end
    else
        if data.type == BreakRuleType.WhiteBallIn or data.type == BreakRuleType.Delay or data.type == BreakRuleType.NotOwnedBall then
            if BilliardsBattle.IsSelfCurPlayer(data.next_player) then
                playerTip = I18nManager.GetCurrentValue("Billiards_FreeBall", true)
            end 
        end
    end

    local prompts = {}
    if strKey ~= "" then
        local str = I18nManager.GetCurrentValue(strKey, true)
        table.insert(prompts, str)
    end

    if playerTip ~= "" then
        table.insert(prompts, playerTip)
    end 

    self:ShowTips(prompts)
end

function UIBilliardsMain:NoticeSeprateBall(data)
    if not UserInfo.IsViewer() then
        for _, player in pairs(BilliardsBattle.GetPlayers()) do
            if player:IsSelf() then
                local str = ""
                if player:GetBallType() == BillardsType.BillardsBallType.SINGLE_BALL then
                    str = I18nManager.GetCurrentValue("Billiards_HitSolidBall", true)
                elseif player:GetBallType() == BillardsType.BillardsBallType.COLOR_BALL then
                    str = I18nManager.GetCurrentValue("Billiards_HitStripBall", true)
                end
                if str ~= "" then 
                    self:ShowTips({[1] = str}) 
                end
                break
            end
        end
    end
end

function UIBilliardsMain:ShowTips(tips, callback)
    local content = {}
    for _, value in ipairs(tips) do
        table.insert(content, {showType = 2, contentStr = value})
    end

    self.root:OpenAsync(UIBilliardsPrompt, UIContext.Get({showContent = content, callback = callback}))
end

function UIBilliardsMain:StopTimer()
    for i = 1, 2 do
        self.view["playerTimeImg_"..i].fillAmount = 1
        self.view["playerTimeImg_"..i].gameObject:SetActive(false)
    end
    
    if self.coutDownTimer then
        self.scheduler:Remove(self.coutDownTimer)
        self.coutDownTimer = nil
    end

    self.isPlayingSound = false
end

function UIBilliardsMain:OnSliderPointUp()
    print("OnSliderPointUp+++++++++++++++", self.m_powerValue, self.leftTime)
    if self.m_powerValue > 0 and self.leftTime > 0 then
        BilliardsBattle.HitBall(false, self.m_powerValue)
        ----停止超时倒计时
        self:StopTimer()
        self:DisableOperation()
    end
    
    self.m_powerValue = 0
    self.view.sliderPower.value = 0
    self.view.sliderPowerCue.rectTransform:SetAnchoredPosition(0, 0)
    self.view.imgPowerCancel:SetActive(false)
end

function UIBilliardsMain:UpdateGameState()
    self:DisableOperation()
    local cur_operaPlayer = BilliardsBattle.GetCurrentPlayer()
    if BilliardsBattle.CheckRoundPhase(BillardsType.RoundPhase.FREE_BALL) then
        local cb = nil
        if BilliardsBattle.IsSelfCurPlayer() then
            cb = function()
                local pos = Billiards8Rule.whiteBallPos
                BilliardsBattle.SetFreeBallPos(pos.x, pos.y, true)
            end
        end
        if cur_operaPlayer then self:ShowTimer(BilliardsBattle.freeBallTime, 15, cur_operaPlayer.pos, cb) end
    elseif BilliardsBattle.CheckRoundPhase(BillardsType.RoundPhase.HIT_BALLS) then
        local cb = nil
        if  BilliardsBattle.IsSelfCurPlayer() then
            self.view.sliderPower.enabled = true
            self.view.angleSlider.enabled = true
            cb = function() 
                BilliardsBattle.HitBall(true)
                self:DisableOperation()
            end
        end
        if cur_operaPlayer then self:ShowTimer(BilliardsBattle.countDowntTime, 30, cur_operaPlayer.pos, cb) end
    else
        self:StopTimer()
    end
end

function UIBilliardsMain:NoticeShotBall(data)
    local Content = {}
    local cur_operaPlayer = nil
    local isSelfCurPlayer = false
    local con_enter_num = 0
    if data then
        cur_operaPlayer = BilliardsBattle.GetPlayerByPos(data.pos)
        isSelfCurPlayer = BilliardsBattle.IsSelfCurPlayer(data.pos)
        con_enter_num = data.continuous_num
    else
        cur_operaPlayer = BilliardsBattle.GetCurrentPlayer()
        isSelfCurPlayer = BilliardsBattle.IsSelfCurPlayer()
        con_enter_num = cur_operaPlayer.con_enter_num
    end

    if con_enter_num > 1 then
        table.insert(Content, {showType = 3, player = cur_operaPlayer})
    end

    if not UserInfo.IsViewer() then
        local strTip = isSelfCurPlayer and I18nManager.GetCurrentValue("Billiards_ChangePlayer", true) 
        or I18nManager.GetCurrentValue("Billiards_OthersRound", true)
        table.insert(Content, {showType = 2, contentStr = strTip})
    end
    
    self.root:OpenAsync(UIBilliardsPrompt, UIContext.Get({showContent = Content}))
end

function UIBilliardsMain:OnDrag(go, eventData, isEnd)
    if go == self.view.angleSlider.gameObject then
        ---自由球情况下不显示杆子
        if BilliardsBattle.CheckRoundPhase(BillardsType.RoundPhase.FREE_BALL) then
            return
        end

        local delta = eventData.delta
        local rate = delta.y / 4680

        local dirX, dirY = BilliardsBattle.GetCueDir()
        local rawImageRect = self.view.angleSlider.uvRect
        rawImageRect.y = rawImageRect.y - 6 * rate
        self.view.angleSlider.uvRect = rawImageRect

        local radian = atan(dirY, dirX) + (pi * 0.5) * rate * 0.6
        local Radius = sqrt(dirX * dirX + dirY * dirY)

        dirX = cos(radian) * Radius
        dirY = sin(radian) * Radius
        BilliardsBattle.SetCueDir(dirX, dirY, isEnd)
    end
end

function UIBilliardsMain:OnEndDrag(go, eventData)
    self:OnDrag(go, eventData, true)
end

function UIBilliardsMain:DisableOperation()
    self.view.sliderPower.enabled = false
    self.view.sliderPower.value = 0

    self.view.angleSlider.enabled = false
    local x, _ = self.view.angleSlider.rectTransform:GetAnchoredPosition()
    self.view.angleSlider.rectTransform:SetAnchoredPosition(x, 0)
end

--- 进球事件
function UIBilliardsMain:OnEnterBall(data)
    --- 刷新角色UI状态
    local holePos = BilliardsTable.holeTrans[data.holeIndex].transform.position

    local uiPos = BillardsUtil.ConvertWorldtoUI(holePos, self.view.mainPanel)

    local spineObj = self.view["spineEnterBall_"..data.holeIndex]

    spineObj:SetActive(true)
    spineObj.transform:SetLocalPosition(uiPos.x, uiPos.y, 0)
    local spineAnim = spineObj.gameObject:GetComponent(typeof(Spine.Unity.SkeletonGraphic))
    spineAnim.AnimationState:SetAnimation(0, "play", false)
end

function UIBilliardsMain:OnValueChanged(com, value)
    if com == self.view.sliderPower then
        local power = math.floor(value * BilliardsValueCfg.MAX_F)
        --- 计算球杆拉伸位置
        BilliardsBattle.SetCuePower(power)
        if value <= 0.25 and power < self.m_powerValue then 
            self.view.imgPowerCancel:SetActive(true)
        else
            self.view.imgPowerCancel:SetActive(false)
        end
        self.m_powerValue = power
        local y = - value * self.view.sliderPowerCue.rectTransform.rect.height
        self.view.sliderPowerCue.rectTransform:SetAnchoredPosition(0, y)
    end
end

function UIBilliardsMain:OnButtonClick(com)
end

function UIBilliardsMain:OnClosed()
    self._tablePos = nil
end

return UIBilliardsMain